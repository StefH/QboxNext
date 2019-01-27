using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using System;
using WindowsAzure.Table.Attributes;

namespace QboxNext.Server.Infrastructure.Azure.Models.Internal
{
    public class MeasurementEntity
    {
        [PartitionKey]
        public string SerialNumber { get; set; }

        [RowKey]
        public string RowKey { get; set; }

        public string CorrelationId { get; set; }

        public DateTime LogTimeStamp { get; set; }

        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public double PulseValue { get; set; }

        public MeasurementEntity()
        {
            // This is needed.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementEntity"/> class.
        /// </summary>
        /// <param name="qboxMeasurement">The measurement.</param>
        public MeasurementEntity([NotNull] QboxMeasurement qboxMeasurement)
        {
            Guard.NotNull(qboxMeasurement, nameof(qboxMeasurement));

            SerialNumber = qboxMeasurement.SerialNumber;
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:d19}:{qboxMeasurement.CounterId:D4}";

            CorrelationId = qboxMeasurement.CorrelationId;
            LogTimeStamp = qboxMeasurement.LogTimeStamp;
            MeasureTime = qboxMeasurement.MeasureTime;
            CounterId = qboxMeasurement.CounterId;
            PulseValue = qboxMeasurement.PulseValue;
        }
    }
}