using System;
using WindowsAzure.Table.Attributes;

namespace QboxNext.Server.Infrastructure.Azure.Models.Internal
{
    public class RegistrationEntity
    {
        [PartitionKey]
        public string SerialNumber { get; set; }

        [RowKey]
        public string SerialNumber2 { get; set; }

        public DateTime RegistrationTimestamp { get; set; }

        public bool FirmwareDownloadAllowed { get; set; }

        public string FirmwareVersion { get; set; }
    }
}