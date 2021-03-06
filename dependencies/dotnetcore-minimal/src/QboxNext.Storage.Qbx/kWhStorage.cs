﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QboxNext.Core.Extensions;
using QboxNext.Core.Utils;

namespace QboxNext.Storage.Qbx
{
    /// <summary>
    /// A persistent storage provider to store records of measurement data
    /// Included in the record are:
    /// - Raw; the actual data received from the qbox
    /// - Value; a value that has been computed from the raw element using some structure. This allows per minute changes to 
    /// the value computation basis but still fast retrieval for graphs
    /// - Money; a monetary value computed from the Value element using some pricing structure
    /// - Quality index; if values are missing in the file then this quality index defines the relative quality (log10).
    /// The files are created when needed and run till no more data is recieved. When the file is too short it will be extended automatically
    /// for another year.
    /// The file consists of slots of one record per minute. By calculating the offset we can jump ahead in the stream to retrieve the
    /// record based on the offset. Storing the record makes the same calculation. Recalculating the file will overwrite the values.
    /// The file slots are initialized to 0 and the quality index is based on the distance between the actual slots that have data in it.
    /// Raw data that is received will always fill up the intermediate slots in the past that have no raw data with the average value calculated
    /// over the missing slots.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class kWhStorage : IStorageProvider
    {
        private readonly ILogger<kWhStorage> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StorageProviderContext _context;
        private readonly kWhStorageOptions _options;

        #region private

        private bool FileExists
        {
            get
            {
                // Calling File.Exists is really expensive, so we first try to find out if we already opened the file.
                if (_reader != null || _writer != null)
                    return true;

                return System.IO.File.Exists(GetFilePath());
            }
        }

        /// <summary>
        /// Backing field for the StartOfFile property. Denotes the first time a value was added and is the base for
        /// the offset calculations when storing values in the file.
        /// </summary>
        private DateTime? _startOfFile;

        /// <summary>
        /// Backing field for the EndOfFile property. The value is persistently stored in the file and read when 
        /// a value is set or if the file was available when the class was created.
        /// It is used to signal when the file needs to be expended.
        /// </summary>
        private DateTime? _endOfFile;

        /// <summary>
        /// Backing field for the Writer property
        /// </summary>
        private SafeWriterWrapper _writer;

        /// <summary>
        /// Lazy creation of BinaryWriter on first call
        /// </summary>
        private BinaryWriter Writer
        {
            get
            {
                if (_writer == null)
                    _writer = new SafeWriterWrapper(_loggerFactory, GetFilePath());
                return _writer.BinaryWriter;
            }
        }

        private BinaryReader _reader;
        /// <summary>
        /// Lazy creation of BinaryReader when first called.
        /// </summary>
        private BinaryReader Reader
        {
            get
            {
                return _reader ?? (_reader = new BinaryReader(System.IO.File.Open(GetFilePath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
            }
        }

        /// <summary>
        /// Settings for the HeaderSize used in the calculations for the offset
        /// </summary>
        private const int HeaderSize = 32;	// 8 bytes start, 8 bytes end, 16 bytes GUID
        /// <summary>
        /// Settings for the recordsize used in the calculations for the offset
        /// </summary>
        private const int RecordSize = 26;  // 8 bytes raw, 8 bytes kWh, 8 bytes money, 2 bytes quality

        private kWhStorageBuffer _buffer;

        #endregion

        #region public

        /// <summary>
        /// A Guid id intended to be used as a unique identifier for the file
        /// Is not used for the moment except in the read header and it's space is used in the calculations for the offset
        /// so cannot be removed for the moment because the structure of the file would then change.
        /// </summary>
        public Guid ID { get; private set; }

        /// <summary>
        /// Stores the value from the header of the file. It is only read when either a value is stored or the file 
        /// was already available when this storage provider was created. It is used in the calculations for the offset
        /// and to determine if the value falls within an acceptable range.
        /// </summary>
        public DateTime StartOfFile
        {
            get
            {
                if (_startOfFile == null)
                    ReadHeader();
                if (_startOfFile == null)
                    return new DateTime();

                return _startOfFile.Value;
            }
        }

        /// <summary>
        /// Property that denotes the date and time the file ends. If you are reading through a file stream and there is no end marker
        /// you can have unexpected results when setting the offset. This will enable the class to expand the file when needed.
        /// </summary>
        public DateTime EndOfFile
        {
            get
            {
                if (_endOfFile == null)
                    ReadHeader();
                if (_endOfFile == null)
                    return new DateTime();

                return _endOfFile.Value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that has an extra parameter to hold the ReferenceDate so the storage can be created in the past. This helps
        /// when tools want to create and recalcalculate files based on history of existing Qboxes or to simulate a past history.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options">The options.</param>
        /// <param name="context"></param>
        public kWhStorage(ILoggerFactory loggerFactory, IOptions<kWhStorageOptions> options, StorageProviderContext context)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<kWhStorage>();
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            ID = Guid.NewGuid();

            _buffer = new kWhStorageBuffer(this);

            _logger.LogDebug("SerialNumber: {0}, filePath: {1}", _context.SerialNumber, GetFilePath());

            if (!FileExists)
                return;

            try
            {
                ReadHeader();
            }
            catch (EndOfStreamException)
            {
                Dispose();
                System.IO.File.Delete(GetFilePath());
            }
        }

        #endregion

        #region private


        /// <summary>
        /// Initializes a file to all 0 so that we can tell that a certain timeslot was not filled with a measurement during 
        /// reading and writing of the data.
        /// </summary>
        /// <param name="startOfFile"></param>
        private void InitializeNewFile(DateTime startOfFile)
        {
            // Write start time. The start time should start evenly spread over the Qbox Counter files because 
            // it takes some time to initialize the file to 4Mb. If all Qboxes have the same starttime for the file
            // they will all start a new file at the same time to.
            Writer.Write(startOfFile.ToBinary());

            // write end of file
            var endOfFile = startOfFile.AddDays(_options.GrowthNrOfDays);
            Writer.Write(endOfFile.ToBinary());

            // write the ID            
            Writer.Write(ID.ToByteArray());

            // Increase the size of the file to create the file till the end of number of days to grow (GrowthNrOfDays)
            var span = endOfFile - startOfFile;
            InitializeToZero(span);
        }

        /// <summary>
        /// Initializes a time slot record to all zero's and writes it to the file
        /// </summary>
        /// <param name="span"></param>
        private void InitializeToZero(TimeSpan span)
        {
            var slots = (int)span.TotalMinutes;
            for (var i = 0; i < slots; i++)
            {
                WriteSlot(Writer, new Record(ulong.MaxValue, 0, 0, 0));
            }
            Writer.Flush();
        }

        /// <summary>
        /// Writes a complete time slot record to the writer's stream at it's present position. The positioning must be done
        /// using GetOffset before calling Writeslot.
        /// </summary>
        /// <param name="writer">The BinaryWriter positioned at the beginning of the time slot record</param>
        /// <param name="record">The record to be written to the file</param>
        private void WriteSlot(BinaryWriter writer, Record record)
        {
#if DEBUG
            _logger.LogTrace("Enter");
            _logger.LogTrace("Raw:{0} | Value:{1} | Money:{2} | Quality:{3}", record.Raw, record.KiloWattHour, record.Money, record.Quality);
#endif
            try
            {
                var kwh = Convert.ToUInt64(record.KiloWattHour * (int)_context.Precision);
                var money = Convert.ToUInt64(record.Money * (int)_context.Precision);
                writer.Write(record.Raw);
                writer.Write(kwh);
                writer.Write(money);
                writer.Write(record.Quality);

                _buffer.Clear();
            }
            catch (Exception e)
            {
                _logger.LogError(e, string.Format("Error: {4} | Raw:{0} | Value:{1} | Money:{2} | Quality:{3}",
                    record.Raw, record.KiloWattHour, record.Money, record.Quality, e.Message));
                throw;
            }
#if DEBUG
            _logger.LogTrace("Exit");
#endif
        }

        /// <summary>
        /// Reads a complete time slot record from the streams current location
        /// </summary>
        /// <param name="reader">BinaryReader positioned at the beginning of the time slot record. Positiong is done
        /// previously using GetOffset</param>
        /// <param name="inTimestamp"></param>
        /// <returns>a new record holding the dat found in the file or NULL if the raw dat was 0. Raw == 0 means there is no measurement in the time slot</returns>
        private Record ReadSlot(DateTime inTimestamp)
        {
            return _buffer.ReadSlot(inTimestamp);
        }

        /// <summary>
        /// Reads the header of the file. The header holds the following information:
        /// - start of file; the date and time the file received it's first measurement
        /// - end of file; when the file is initialized it is done so for a year worth of data. Every time a measurement is received
        /// larger then this, the file is extended for another year
        /// - id; guid to maybe identify the file (not used in the current setup)
        /// </summary>
        private void ReadHeader()
        {
            _logger.LogTrace("Enter");

            Reader.BaseStream.Seek(0, SeekOrigin.Begin);
            // Tijden in hele minuten
            _startOfFile = DateTime.FromBinary(Reader.ReadInt64()).TruncateToMinute();
            _endOfFile = DateTime.FromBinary(Reader.ReadInt64()).TruncateToMinute();
            ID = new Guid(Reader.ReadBytes(16));

            _logger.LogTrace("{0} header start {1} end {2}, filesize {3}  calculated endtime {4}", GetFilePath(), _startOfFile, _endOfFile, Reader.BaseStream.Length, CalculatedEndTime);
            _logger.LogTrace("Exit");
        }

        /// <summary>
        /// Calculate the offset, throws an exception if it's out of bounds.
        /// </summary>
        private long CalculateSafeOffset(DateTime pulseTime)
        {
            var offset = CalculateOffset(pulseTime);
            CheckOffset(offset);
            return offset;
        }


        /// <summary>
        /// Calculates the offset for the stream reader based in the nr of minutes from the start of the file and
        /// the record size.
        /// </summary>
        private long CalculateOffset(DateTime pulseTime)
        {
            var period = pulseTime - StartOfFile;
            var offset = HeaderSize + ((int)period.TotalMinutes * RecordSize);

            return offset;
        }

        /// <summary>
        /// Check the offset and throw an exception if it's out of bounds.
        /// </summary>
        private void CheckOffset(long inOffset)
        {
            if (IsOffsetTooHigh(inOffset))
            {
                var error = string.Format("Offset after end of file ({0}) - offset {1} [file is limited to {2} - {3}]", GetFilePath(), inOffset, CalculateOffset(StartOfFile), CalculateOffset(EndOfFile));
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            if (IsOffsetTooLow(inOffset))
            {
                var error = string.Format("Offset before start of file ({0}) - offset {1} [file is limited to {2} - {3}]", GetFilePath(), inOffset, CalculateOffset(StartOfFile), CalculateOffset(EndOfFile));
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }
        }


        /// <summary>
        /// Is the given offset valid?
        /// </summary>
        private bool IsOffsetValid(long inOffset)
        {
            return !IsOffsetTooHigh(inOffset) && !IsOffsetTooLow(inOffset);
        }


        /// <summary>
        /// Is the offset located beyond the end of the file?
        /// </summary>
        private bool IsOffsetTooHigh(long inOffset)
        {
            return inOffset >= HeaderSize + (EndOfFile - StartOfFile).TotalMinutes * RecordSize;
        }


        /// <summary>
        /// Is the offset located before the data part of the file?
        /// </summary>
        private bool IsOffsetTooLow(long inOffset)
        {
            return inOffset < HeaderSize;
        }

        /// <summary>
        /// Calculated endtime based on length of stream
        /// </summary>
        /// <returns></returns>
        private DateTime CalculatedEndTime
        {
            get
            {
                var totalMinutes = (Reader.BaseStream.Length - HeaderSize) / RecordSize;
                return StartOfFile.AddMinutes(totalMinutes);
            }
        }

        private string GetDirectory()
        {
            return Path.Combine(_options.DataStorePath, "Qbox_" + _context.SerialNumber);
        }

        /// <summary>
        /// Checks the target directory exists.
        /// </summary>
        private void CheckTargetDirectoryExists()
        {
            var path = GetDirectory();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Ensures the file exists for the given measurement time.
        /// </summary>
        /// <param name="measurementTime">The measurement time.</param>
        private void EnsureFileExists(DateTime measurementTime)
        {
            CheckTargetDirectoryExists();
            if (!FileExists)
            {
                InitializeNewFile(measurementTime);
            }
            ReadHeader();
            EnsureFileLength(measurementTime);
        }

        /// <summary>
        /// Ensures the length of the file. By examining the end of file marker and the given measurement time.
        /// It extends the file size and initializes the file (new part) to ulong Max Value.
        /// </summary>
        /// <param name="measurementTime">The measurement time.</param>
        private void EnsureFileLength(DateTime measurementTime)
        {
            var endTime = CalculatedEndTime;
            // We want to able to store the complete minute in the file, that means that we have to check if the end of the slot fits in the file.
            if (endTime >= measurementTime.AddMinutes(1))
                return;

            var offsetToEndFile = CalculateOffset(endTime);

            Writer.Seek((int)(offsetToEndFile), SeekOrigin.Begin);
            var newEndTime = measurementTime.AddDays(_options.GrowthNrOfDays);
            InitializeToZero(newEndTime - endTime);
            _endOfFile = newEndTime;

            _logger.LogInformation("File extended till " + _endOfFile);

            WriteEndOfFile(_endOfFile.Value);
        }

        /// <summary>
        /// Writes the end of file marker in the header.
        /// </summary>
        /// <param name="end">The end of file datetime.</param>
        private void WriteEndOfFile(DateTime end)
        {
            Writer.Seek(8, SeekOrigin.Begin);
            Writer.Write(end.ToBinary());
            Writer.Flush();
        }

        /// <summary>
        /// returns the closest previous or next value to the measuretime
        /// </summary>
        /// <param name="measureTime">The time of the measurement</param>
        /// <param name="forward">Tells the search to look ahead iso backwards</param>
        /// <returns>The measurement record</returns>
        private Record GetClosestValue(DateTime measureTime, bool forward)
        {
            var result = GetValue(measureTime);
            if ((result == null) || (result.Raw == ulong.MaxValue && result.Quality == 0))
            {
                if (forward)
                {
                    FindNext(measureTime, out result);
                }
                else
                {
                    FindPrevious(measureTime, out result);
                }
            }
            return result;
        }


        /// <summary>
        /// Find the previous record.
        /// </summary>
        public Record FindPrevious(DateTime inMeasurementTime)
        {
            Record lastValue;
            FindPrevious(inMeasurementTime.TruncateToMinute(), out lastValue);
            return lastValue;
        }


        /// <summary>
        /// Finds the previous record and calculates the distance between the given datetime and the one before that.
        /// </summary>
        /// <param name="inMeasureTime">The measure time.</param>
        /// <param name="outPrevious">The previous.</param>
        /// <returns>The distance between the two records in minutes</returns>
        private int FindPrevious(DateTime inMeasureTime, out Record outPrevious)
        {
            _logger.LogTrace("Enter");
            var distance = 1;

            Guard.IsTrue(inMeasureTime.Second == 0, "inMeasureTime should be truncated to 1 minute");
            var timestamp = inMeasureTime.AddMinutes(-1);
            while (IsExistingSlot(timestamp))
            {
                if (_buffer.IsValidSlot(timestamp))
                {
                    outPrevious = _buffer.ReadSlot(timestamp);
                    _logger.LogTrace("Return: {0}, {1}, {2}, {3}, {4}", distance, outPrevious.Raw, outPrevious.KiloWattHour, outPrevious.Money, outPrevious.Quality);
                    return distance;
                }
                distance++;
                timestamp = timestamp.AddMinutes(-1);
            }

            _logger.LogTrace("Return: {0}, no previous value", distance);
            outPrevious = null;
            return distance;
        }


        /// <summary>
        /// Reads the file for a next value. Depending on the content of the raw it will determine if the record has been written to
        /// and return the distance between the measuretime received and the next record found
        /// </summary>
        /// <param name="measureTime">The time for the measurement for which to find the next record</param>
        /// <param name="next">An out parameter that will hold the values for the next record after returning from the method</param>
        /// <returns>A distance in minutes between the measuretime and the next record</returns>
        private int FindNext(DateTime measureTime, out Record next)
        {
            _logger.LogTrace("Enter");

            var distance = 0;

            while ((next = GetValue(measureTime.AddMinutes(distance * 1))) != null)
            {
                if (next.Raw < ulong.MaxValue)
                    return distance;
                distance++;
            }
            _logger.LogTrace("Return: {0}", distance);
            return distance;
        }

        /// <summary>
        /// Calculates the delta between two measurements. For every resolution smaller than the hour we need to remove
        /// the time element because the kWh is stored.
        /// </summary>
        /// <param name="first">The first or begin record</param>
        /// <param name="last">The last or end record</param>
        /// <param name="eenheid">The unit that the calucalation is for</param>
        /// <returns>Returns a decimal containing the delta. For unit kWh with resolution smaller than an hour, the amount is returned in W.
        /// For unit with resolution greater than or equal to an hour, the amount is returned in Wh. For unit m3, the amount is returned in liters.</returns>
        private static decimal CalculateDelta(Record first, Record last, Unit eenheid)
        {
            switch (eenheid)
            {
                case Unit.Raw:
                    throw new Exception("Cannot calculate delta from Raw values");
                case Unit.kWh:
                    // TotalMinutes is een double
                    var minutes = (int)(last.Time - first.Time).TotalMinutes;
                    minutes = minutes <= 0 ? 1 : minutes;
                    // SAM: factor is a nasty hack to show power in day graph and energy in other graphs.
                    // We should probably fix it by replacing the unit types kWh and M3 with Energy and
                    // adding a new one, Power.
                    var factor = Convert.ToDecimal(minutes < 60d ? (60.0 / minutes) : 1.0);
                    return ((last.KiloWattHour - first.KiloWattHour) * factor) * 1000m;
                case Unit.M3:
                    return (last.KiloWattHour - first.KiloWattHour) * 1000m;
                default:
                    throw new ArgumentOutOfRangeException("eenheid");
            }
        }

        /// <summary>
        /// Calculates if the time is allowed (within the ranges of the file)
        /// The Qbox has a dodgy timer so sometimes measurements times come in that are very off.
        /// </summary>
        /// <param name="measureTime">The time for the measurement to check</param>
        /// <returns>true if the time is smaller then the Reference date (-5) and the datetime should be after 1-1-2010</returns>
        //refactor: investigate the use of the method
        private bool IsTimeAllowed(DateTime measureTime)
        {
            return measureTime <= _context.ReferenceDate.AddMinutes(5) && measureTime >= new DateTime(2010, 1, 1);
        }

        #endregion

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_reader != null)
                _reader.Dispose();
            if (_writer != null)
                _writer.Dispose();
            _reader = null;
            _writer = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~kWhStorage()
        {
            Dispose(false);
        }

        #endregion


        /// <summary>
        /// Return a full path based on the filepath and the year we are in now finished off with the extension.
        /// This should refactored to remove the year from the filename because the storage now creates endless files iso
        /// 1 file per year.
        /// </summary>
        //refactor: remove year dependency in the file name and merge the StorageId function
        public string GetFilePath()
        {
            // Create the filename
            var filename = _context.StorageId == StorageId.Empty ? 
                $"{_context.SerialNumber}_{_context.CounterId:00000000}{_options.Extension}" :
                $"{_context.StorageId}{_options.Extension}";
            var result = Path.GetFullPath(Path.Combine(GetDirectory(), filename));
            return result;
        }

        /// <summary>
        /// Returns the sum of the values for the given Unit for the given period.
        /// The sum is calculated by calculating the delta between two values. this makes this
        /// function not usable in case of the Raw value. The Raw value is not a running total.
        /// If the records for the requested begin and end are not found the function tries to find the
        /// closest values available.
        /// </summary>
        /// <param name="begin">Begin date time of the period to sum</param>
        /// <param name="end">End date time of the period to sum</param>
        /// <param name="eenheid">The unit of the values that should be summed (kWh, Money)</param>
        /// <returns>The sum (delta) of the running total for the given unit</returns>
        public decimal Sum(DateTime begin, DateTime end, Unit eenheid)
        {
            begin = begin.TruncateToMinute();
            end = end.TruncateToMinute();

            if (!FileExists)
            {
                return 0;
            }
            var start = begin < StartOfFile ? StartOfFile : begin;
            var stop = end > EndOfFile ? EndOfFile : end;
            stop = stop > _context.ReferenceDate ? _context.ReferenceDate : stop;
            if (stop < start)
            {
                return 0;
            }

            var first = GetClosestValue(start, true);
            if (first == null)
                return 0;
            var last = GetClosestValue(stop, false);
            if (last == null)
                return 0;

            return CalculateDelta(first, last, eenheid);
        }

        /// <summary>
        /// Gets the records from the file. This uses an alogrythm to increase data quality.
        /// </summary>
        /// <param name="inBegin">The begin of the period to read from the file.</param>
        /// <param name="inEnd">The end of the period to read from the file.</param>
        /// <param name="inUnit">The unit that the records will contain</param>
        /// <param name="ioSlots">A list of series values that are used to put the records in. This allows for empty "buckets" 
        ///  so the caller can always expect a correct period division in the resolution that was given. The series value can have 
        /// values from othe counters (combined devices)
        ///  see the SeriesValue for more info</param>
        /// <param name="inNegate">In case the call is made from a Device that has multiple counters and this is a negative counter 
        /// the values are negated in the resulting seriesvalue</param>
        /// <returns></returns>
        public bool GetRecords(DateTime inBegin, DateTime inEnd, Unit inUnit, IList<SeriesValue> ioSlots, bool inNegate)
        {
            inBegin = inBegin.TruncateToMinute();
            inEnd = inEnd.TruncateToMinute();

            if (inBegin > inEnd)
                throw new ArgumentOutOfRangeException("inBegin", "begin before end");

            _logger.LogTrace("GetRecords(begin = {0}, end = {1}, counter = {2}, path = {3}, refdate = {4})", inBegin, inEnd, _context.CounterId, GetFilePath(), _context.ReferenceDate);
            if (!FileExists)
            {
                _logger.LogWarning("File does not exist {0}", GetFilePath());
                return false;
            }

            SeriesValue previousSlot = null;
            Record previousLast = null;
            foreach (var currentSlot in ioSlots)
            {
                _logger.LogTrace("process slot {0} - {1}", currentSlot.Begin, currentSlot.End);
                var beginTime = currentSlot.Begin < inBegin ? inBegin : currentSlot.Begin;
                var endTime = currentSlot.End > inEnd ? inEnd : (currentSlot.End < beginTime ? beginTime : currentSlot.End);
                _logger.LogTrace("beginTime = {0}, endTime = {1}", beginTime, endTime);

                Record first;
                // Optimization: use previous last value if the end of the previous slot is the start of the current slot,
                // this prevents an unneeded extra file read.
                if (previousSlot != null && previousLast != null && previousLast.IsValidMeasurement && previousSlot.End == beginTime)
                    first = previousLast;
                else
                    first = beginTime < StartOfFile ? GetClosestValue(StartOfFile, true) : GetValue(beginTime);

                // When for some reason the value contains an invalid measurement, search for the next valid measurement.
                if (first != null && !first.IsValidMeasurement)
                {
                    first = GetClosestValue(beginTime, true);
                    if (first != null && first.Time > endTime)
                        continue;
                }

                _logger.LogTrace("first = {0}", first != null ? first.Raw.ToString(CultureInfo.InvariantCulture) : "null");
                // todo (evalueren): als eerste waarde niet gevonden wordt heeft verder zoeken geen zin??!!
                if (first == null)
                    break;

                var lastAllowedTimestamp = _context.ReferenceDate < EndOfFile ? _context.ReferenceDate : EndOfFile;
                var last = endTime > lastAllowedTimestamp ? GetClosestValue(lastAllowedTimestamp, false) : GetValue(endTime);

                _logger.LogTrace("last = {0}", last != null ? last.Raw.ToString() : "null");
                // Bij dag of maand resolution zoeken naar dichtsbijzijnde waarde indien first en last geen waarde heeft (qplat-73)
                if (first.Time <= endTime && first.IsValidMeasurement && (last == null || !last.IsValidMeasurement) && ((currentSlot.End - currentSlot.Begin).TotalDays >= 1.0))
                    last = GetClosestValue(endTime, false);

                if ((last != null && beginTime < EndOfFile && first.Time < last.Time) &&
                    (first.IsValidMeasurement) && (last.IsValidMeasurement))
                {
                    var delta = CalculateDelta(first, last, inUnit);
                    _logger.LogTrace("delta = {0}", delta);
                    currentSlot.Value = Convert.ToDecimal(inNegate ? delta * -1m : delta);
                    if (endTime > EndOfFile)
                    {
                        break;
                    }
                }

                previousSlot = currentSlot;
                previousLast = last;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var seriesValue in ioSlots)
                    _logger.LogDebug("{0} - {1} : {2}", seriesValue.Begin, seriesValue.End, seriesValue.Value);
            }
            _logger.LogTrace("Exit");
            return true;
        }


        private bool IsExistingSlot(DateTime inTimestamp)
        {
            if (!FileExists)
                return false;

            return (inTimestamp >= StartOfFile && inTimestamp < EndOfFile && inTimestamp <= _context.ReferenceDate.AddMinutes(5));
        }


        /// <summary>
        /// Gets the value for a specified measurement time .
        /// </summary>        
        /// <param name="measureTime">The measure time.</param>
        /// <returns>The record found at the specified time or null if the time falls outside the current range of the file</returns>
        public Record GetValue(DateTime measureTime)
        {
            measureTime = measureTime.TruncateToMinute();

            if (!FileExists)
            {
                return null;
            }
            try
            {
                if (measureTime < StartOfFile || measureTime >= EndOfFile || measureTime > _context.ReferenceDate.AddMinutes(5))
                {
                    _logger.LogWarning("measureTime {1} falls outside the file ({0}) s:{2} e:{3}", GetFilePath(), measureTime, StartOfFile, EndOfFile);
                    return null;
                }

                var result = ReadSlot(measureTime);
                result.Time = measureTime;
                _logger.LogTrace("Read {0} for timestamp {1}", result.Raw, measureTime);
                return result;
            }
            catch (Exception ex)
            {
                // Ondanks test of measure time binnen start en end tijd van de stream valt kan er Seek uitgevoerd worden, 
                // de endtime komt niet overeen met de waarden in de header ....
                if (ex is EndOfStreamException)
                    throw;

                //todo: on reading we do not want any errors popping up? yes in case reading outside the file
                return null;
            }
        }

        /// <summary>
        /// Sets the value for the given measurement time by calculating the value for kWh, euro and quality index.
        /// It creates a delta for the pulses and calculates the running total.
        /// It fills the gaps(if any) with averages and adds the quality index. 
        /// </summary>
        /// <param name="inMeasureTime">the time of the measurement</param>
        /// <param name="inPulseValue">the raw pulse value</param>
        /// <param name="inPulsesPerUnit">the formula to calculate the kWh from</param>
        /// <param name="inEurocentsPerUnit">The formula to calculate the value in Euro's</param>
        /// <param name="inRunningTotal">This value is used for efficiency so the function does not need to try
        /// and find the current value. With large files and longs time betwee set value finding the value will degrade
        /// performance</param>
        public Record SetValue(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit, decimal inEurocentsPerUnit, Record inRunningTotal = null)
        {
            inMeasureTime = inMeasureTime.TruncateToMinute();

            _logger.LogTrace("Enter");

            if (!IsTimeAllowed(inMeasureTime))
                return null;

            EnsureFileExists(inMeasureTime);

            inPulsesPerUnit = inPulsesPerUnit == 0 ? 1 : inPulsesPerUnit;
            inEurocentsPerUnit = inEurocentsPerUnit == 0 ? 1 : inEurocentsPerUnit;

            // find the first value previous to this new value but not before the beginning of the file

            var previous = inRunningTotal;
            Record current = null;
            var distance = previous == null ?
                FindPrevious(inMeasureTime, out previous) : // the distance is returned 
                (int)((inMeasureTime - previous.Time).TotalMinutes + 0.5);// calculate the distance from the previous time
            if (distance > 0)
            {
                if (previous == null)
                {
                    Writer.BaseStream.Seek(CalculateSafeOffset(inMeasureTime), SeekOrigin.Begin);

                    var delta = inPulseValue / inPulsesPerUnit;
                    var kWhValue = delta;
                    var euroValue = delta * inEurocentsPerUnit;
                    current = new Record(inPulseValue, kWhValue, euroValue, 0);
                    WriteSlot(Writer, current);
                }
                else
                {
                    _logger.LogTrace("distance: {0}", distance);
                    // calculate the qualityindex
                    var quality = distance == 0 ? (ushort)0 : Convert.ToUInt16(Math.Log10(distance) * 10000);

                    _logger.LogTrace("distance: {0}, quality: {1}, formulekWh: {2}", distance, quality, inPulsesPerUnit);

                    var delta = inPulseValue < previous.Raw ? 0m : inPulseValue - previous.Raw;
                    var deltakWh = delta / inPulsesPerUnit;
                    var lastValue = previous.KiloWattHour;
                    var lastMoney = previous.Money;

                    _logger.LogTrace("delta: {0}, lastkWhValue: {1}, lastMoney: {2}", delta, lastValue, lastMoney);

                    if (distance <= 1)
                    {
                        lastValue += deltakWh;
                        lastMoney += deltakWh * inEurocentsPerUnit;

                        current = new Record(inPulseValue, lastValue, lastMoney, quality);
                        _logger.LogTrace("distance = 0 >> raw:{0} | kWh: {1} | money: {2} | quality: {3}",
                            inPulseValue, current.KiloWattHour,
                            current.Money, current.Quality);

                        Writer.BaseStream.Seek(CalculateSafeOffset(inMeasureTime), SeekOrigin.Begin);
                        WriteSlot(Writer, current);
                    }
                    else
                    {
                        // set the pointer of the file to the beginning of the missing values interval
                        Writer.BaseStream.Seek(CalculateSafeOffset(inMeasureTime.AddMinutes((distance - 1) * -1)), SeekOrigin.Begin);

                        var average = (ulong)(delta / distance);
                        var rest = delta % distance;
                        var raw = ulong.MaxValue;
                        for (var i = 0; i < distance; i++)
                        {
                            var value = average;
                            if (i < distance - 1 && rest > 0)
                            {
                                // spread out the rest value over the interval
                                value = rest > 0 ? average + 1 : average;
                                rest--;
                            }
                            else if (i == distance - 1)
                            {
                                raw = inPulseValue;
                                quality = 0;
                            }
                            var valuekWh = value / inPulsesPerUnit;

                            lastValue += valuekWh;
                            lastMoney += valuekWh * inEurocentsPerUnit;

                            current = new Record(raw, lastValue, lastMoney, quality);
                            _logger.LogTrace("distance = {4} >>>>>> raw:{0} | kWh: {1} | money: {2} | quality: {3}", raw,
                                current.KiloWattHour, current.Money, current.Quality, distance);

                            WriteSlot(Writer, current);
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Overwrite attempt: {0}", inMeasureTime);
            }
            Writer.Flush();
            _logger.LogTrace("Exit");
            if (current != null)
                current.Time = inMeasureTime;
            return current;
        }


        /// <summary>
        /// To initialize the file we need to set the raw value to max value
        /// FindNext and FindPrevious are depending on this value to be there to find out if
        /// the value was written by a Qbox message or not. We cannot leave the file uninitialized 
        /// because we read binary streams without markers we have no way to see what the next or previous value would be.
        /// </summary>
        /// <param name="inFrom">The starttime to to reinitialize the slots. This does not need to be the beginning of the file (StartOfFile)</param>
        public void ReinitializeSlots(DateTime inFrom)
        {
            _logger.LogTrace("Enter");

            try
            {
                // If the file does not exist yet, there are no slots to reinitialize.
                if (!FileExists)
                    return;

                var span = EndOfFile - inFrom;
                var offset = CalculateOffset(inFrom);
                if (!IsOffsetValid(offset))
                    return;

                _logger.LogDebug("Total minutes: {0}, from: {1}, end of file: {2}", span.TotalMinutes, inFrom, EndOfFile);
                Writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                for (var i = 0; i < span.TotalMinutes; i++)
                    WriteSlot(Writer, new Record(ulong.MaxValue, 0, 0, 0));
            }
            finally
            {
                _logger.LogTrace("Exit");
            }
        }


        /// <summary>
        /// Create a record from values as they are read from file or memory.
        /// </summary>
        private Record CreateRecord(ulong inRaw, ulong inEnergy, ulong inMoney, ushort inQuality, DateTime inTimestamp)
        {
            return new Record(inRaw, inEnergy / (decimal)_context.Precision, inMoney / (decimal)_context.Precision, inQuality)
            {
                Time = inTimestamp
            };
        }


        /// <summary>
        /// Determine if a slot contains a valid measurement from the raw value and the quality.
        /// </summary>
        private bool IsValidSlot(ulong inRaw, ushort inQuality)
        {
            return (inRaw < ulong.MaxValue || (inRaw == ulong.MaxValue && (inQuality > 0 && !_context.AllowOverwrite)));
        }


        public void Merge(kWhStorage storageToAppend)
        {
            var timestamp = storageToAppend.StartOfFile;
            while (storageToAppend.IsExistingSlot(timestamp))
            {
                Record record = storageToAppend.ReadSlot(timestamp);
                if (record.Raw != ulong.MaxValue)
                {
                    Record storedRecord = SetValue(timestamp, record.Raw, 1000, 21);
                    if (storedRecord == null)
                    {
                        throw new StorageWriteException($"Could not store record ${timestamp}:${record.Raw}");
                    }
                }
                timestamp = timestamp.AddMinutes(1);
            }
        }


        /// <summary>
        /// Class for buffered reading of slots.
        /// </summary>
        private class kWhStorageBuffer
        {
            public kWhStorageBuffer(kWhStorage inKwhStorage)
            {
                _kwhStorage = inKwhStorage;
                Clear();
            }


            public void Clear()
            {
                _buffer = null;
                _startSectorOfBuffer = -1;
                _endSectorOfBuffer = -1;
            }


            public Record ReadSlot(DateTime inTimestamp)
            {
                var offsetInBuffer = MakeSureSectorsAreLoaded(inTimestamp);

                var raw = BitConverter.ToUInt64(_buffer, offsetInBuffer);
                var energy = BitConverter.ToUInt64(_buffer, offsetInBuffer + 8);
                var money = BitConverter.ToUInt64(_buffer, offsetInBuffer + 16);
                var quality = BitConverter.ToUInt16(_buffer, offsetInBuffer + 24);

                return _kwhStorage.CreateRecord(raw, energy, money, quality, inTimestamp);
            }


            public bool IsValidSlot(DateTime inTimestamp)
            {
                var offsetInBuffer = MakeSureSectorsAreLoaded(inTimestamp);
                var raw = BitConverter.ToUInt64(_buffer, offsetInBuffer);
                var quality = BitConverter.ToUInt16(_buffer, offsetInBuffer + 24);
                return _kwhStorage.IsValidSlot(raw, quality);
            }


            /// <summary>
            /// Make sure the sectors needed to read the slot at inTimestamp are loaded.
            /// </summary>
            /// <returns>Offset of the slot in the buffer.</returns>
            private int MakeSureSectorsAreLoaded(DateTime inTimestamp)
            {
                long timestampStartFileOffset = _kwhStorage.CalculateSafeOffset(inTimestamp);
                // We have to compute the sector in which the last byte of the slot resides, hence the -1.
                long timestampEndFileOffset = timestampStartFileOffset + RecordSize - 1;

                var startSector = (int)(timestampStartFileOffset / SectorSize);
                var endSector = (int)(timestampEndFileOffset / SectorSize);

                if (startSector < _startSectorOfBuffer || endSector > _endSectorOfBuffer)
                {
                    var bufferStartFileOffset = startSector * SectorSize;
                    if (_kwhStorage.Reader.BaseStream.Seek(bufferStartFileOffset, SeekOrigin.Begin) != bufferStartFileOffset)
                        throw new IOException(String.Format("Could not seek to position {0}", bufferStartFileOffset));

                    // In contrast to timestampEndFileOffset, bufferEndFileOffset points to the first byte NOT in the buffer anymore.
                    var bufferEndFileOffset = (endSector + 1) * SectorSize;
                    var nrBytes = bufferEndFileOffset - bufferStartFileOffset;
                    _buffer = _kwhStorage.Reader.ReadBytes(nrBytes);

                    _startSectorOfBuffer = startSector;
                    _endSectorOfBuffer = endSector;
                }

                return (int)(timestampStartFileOffset - _startSectorOfBuffer * SectorSize);
            }


            private readonly kWhStorage _kwhStorage;
            private const int SectorSize = 4 * 1024;
            private int _startSectorOfBuffer = -1;
            private int _endSectorOfBuffer = -1;
            private byte[] _buffer;
        }
    }
}