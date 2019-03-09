using System;
using WindowsAzure.Table.Attributes;

namespace QboxNext.Server.Infrastructure.Azure.Models.Internal
{
    public class MeasurementEntity
    {
        [PartitionKey]
        public string PartitionKey { get; set; }

        [RowKey]
        public string RowKey { get; set; }

        public string SerialNumber { get; set; }

        public string CorrelationId { get; set; }

        public DateTime MeasureTime { get; set; }

        public bool MeasureTimeAdjusted { get; set; }

        public int? Counter0181 { get; set; }

        public int? Counter0182 { get; set; }

        public int? Counter0281 { get; set; }

        public int? Counter0282 { get; set; }

        public int? Counter2421 { get; set; }
    }
}