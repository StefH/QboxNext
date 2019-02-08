using QboxNext.Qboxes.Parsing.Protocols;
using System.Collections.Generic;
using QboxNext.Storage;

namespace QboxNext.Model.Qboxes
{
    public class Mini
    {
        public string SerialNumber { get; set; }

        public IList<Counter> Counters { get; set; }

        public DeviceMeterType MeterType { get; set; }

        public DeviceMeterType SecondaryMeterType { get; set; }

        public IList<ClientQbox> Clients { get; set; }

        /// <summary>
        /// QboxStatus holds all the data that is needed to determine the health of the Qbox
        /// The information is gathered during the message dump and can be stored seperately from
        /// the Eco based data for performance or access reasons.
        /// </summary>
        public QboxStatus QboxStatus { get; set; }

        public MiniState State { get; set; }

        /// <summary>
        /// The second of each minute that the Qbox should report. This is used to spread the load of all Qboxes across each minute.
        /// </summary>
        public byte Offset { get; set; }

        /// <summary>
        /// The precision used to write to the datastore. Normally mWh.
        /// </summary>
        public Precision Precision { get; set; }

        /// <summary>
        /// Should the Qbox receive the automatic responses? Like setting the meter type after a factory reset. Normally true.
        /// </summary>
        public bool AutoAnswer { get; set; }

        /// <summary>
        /// Holds last meter settings set in command-tab detail qbox form, settings are needed after ResetFactoryDefaults
        /// </summary>
        public string MeterSettings { get; set; }

        public Mini()
        {
            Counters = new List<Counter>();
            QboxStatus = new QboxStatus();
            Clients = new List<ClientQbox>();
        }

        public static string WriteMeterType(DeviceMeterType deviceMeterType)
        {
            var result = new BaseParseResult();
            result.Write((byte)3);
            var meterType = (byte)deviceMeterType;
            result.Write(meterType);
            return result.GetMessage();
        }

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

        public void SetStorageProvider(IStorageProviderFactory storageProviderFactory)
        {
            foreach (var counterPoco in Counters)
            {
                counterPoco.StorageProvider = storageProviderFactory.GetStorageProvider(
                    SerialNumber, 
                    counterPoco.CounterId, 
                    Precision, 
                    counterPoco.StorageId
                );
            }
        }

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
    }
}