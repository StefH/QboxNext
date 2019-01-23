using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using System;

namespace QboxNext.Server.Infrastructure.Azure.Models.Internal
{
    public class MeasurementEntity : TableEntity
    {
        public string CorrelationId { get; set; }

        public DateTime LogTimeStamp { get; set; }

        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public double PulseValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementEntity"/> class.
        ///
        /// Needed because:
        /// error CS0310: 'MeasurementEntity' must be a non-abstract type with a public parameterless constructor in order to use it as
        /// parameter 'TElement' in the generic type or method 'CloudTable.ExecuteQuery{TElement}(TableQuery{TElement}, TableRequestOptions, OperationContext)'
        /// </summary>
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

            PartitionKey = qboxMeasurement.SerialNumber;
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:d19}:{qboxMeasurement.CounterId:D4}";

            CorrelationId = qboxMeasurement.CorrelationId;
            LogTimeStamp = qboxMeasurement.LogTimeStamp;
            MeasureTime = qboxMeasurement.MeasureTime;
            CounterId = qboxMeasurement.CounterId;
            PulseValue = qboxMeasurement.PulseValue;
        }
    }
}
