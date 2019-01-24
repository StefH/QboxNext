using System.Collections.Generic;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.Qserver.Core.Model
{
    /// <summary>
    /// Class for holding the data of the Qbox Mini.
    /// During handling of the messages that the Qbox sends to the server, this class
    /// can be used iso of the full Eco based object. Thus we can return the EcoSpace
    /// quickly to the pool for the next message. This will make performance increase and
    /// less dependency on the number of EcoSpaces in the pool and threads per CPU.
    /// </summary>
    public class MiniPoco
    {
        public string Id { get; set; }
        public string SerialNumber { get; set; } //refactor: niet nodig in Redis
        public string FirmwareRevisionFileName { get; set; } //refactor: only needed when upgrading
        public byte Offset { get; set; } //refactor: calculate value iso human entry
        public bool ExtendedLogging { get; set; } //Refactor: IsLogEnabled en ExtendedLogging kunnen worden samengebracht in QboxesLogLevel 
        public bool IsLogEnabled { get; set; }
        public QboxLogLevel QboxesLogLevel { get; set; }
        public string DataStorePath { get; set; }
        public MiniState State { get; set; }
        public Precision Precision { get; set; }


        /// <summary>
        /// QboxStatus holds all the data that is needed to determine the health of the Qbox
        /// The information is gathered during the message dump and can be stored seperately from
        /// the Eco based data for performance or access reasons.
        /// </summary>
        public QboxStatus QboxStatus { get; set; }


        public IEnumerable<CounterPoco> Counters { get; set; }


        /// <summary>
        /// Prepare the counters so they can be used.
        /// </summary>
        /// <remarks>
        /// We need to set the QboxSerial because that will be part of the key used to store the last value.
        /// </remarks>
        public void PrepareCounters()
        {
            foreach (var counter in Counters)
            {
                counter.QboxSerial = SerialNumber;
                counter.ComposeStorageId();
            }
        }


        /// <summary>
        /// Introduced in A34
        /// </summary>
        public IEnumerable<ClientQboxPoco> Clients { get; set; }


        public DeviceMeterType MeterType { get; set; }
        public DeviceMeterType SecondaryMeterType { get; set; }


        /// <summary>
        /// Check if inMeterType is present in the Qbox topology.
        /// </summary>
        public bool IsMeterTypePresent(DeviceMeterType inMeterType)
        {
            if (MeterType == inMeterType)
                return true;

            foreach (var client in Clients)
            {
                if (client.MeterType == inMeterType)
                    return true;
            }

            return false;
        }


        public bool AutoAnswer { get; set; }


        /// <summary>
        /// Holds the fully defined Command string that can be added to the message result.        
        /// </summary>
        public string NextUpCommandString { get; set; }


        /// <summary>
        /// Holds last meter settings set in command-tab detail qbox form, settings are needed after ResetFactoryDefaults
        /// </summary>
        public string MeterSettings { get; set; }


        /// <summary>
        /// The Storage Provider is an Enum to identify which StorageProvider will store the data 
        /// for the Mini's counters. This value is used by the CounterPoco class to retrieve the 
        /// StorageProvider from the factory.
        /// </summary>
        public StorageProvider StorageProvider { get; set; }


        public MiniPoco()
        {
            QboxStatus = new QboxStatus();
        }


        public void SetStorageProvider()
        {
            foreach (var counterPoco in Counters)
            {
                counterPoco.StorageProvider = StorageProviderFactory.GetStorageProvider(
                    false, StorageProvider, SerialNumber, DataStorePath,
                    counterPoco.CounterId, Precision, counterPoco.StorageId);
            }
        }


        public void SetStorageProvider(IStorageProvider inStorageProvider)
        {
            foreach (var counterPoco in Counters)
                counterPoco.StorageProvider = inStorageProvider;
        }


        /// <summary>
        /// Remove all cached last values of the counters.
        /// </summary>
        public void RemoveLastValues()
        {
            foreach (var counterPoco in Counters)
                counterPoco.RemoveLastValue();
        }
    }
}