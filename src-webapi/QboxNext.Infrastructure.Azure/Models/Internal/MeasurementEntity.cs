using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Common.Validation;
using QboxNext.Domain;
using System;

namespace QboxNext.Infrastructure.Azure.Models.Internal
{
    public class MeasurementEntity : TableEntity
    {
        public Guid CorrelationId { get; set; }

        public DateTime LogTime { get; set; }

        public string ProductNumber { get; set; }

        public string SerialNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public double PulseValue { get; set; }

        public double PulsesPerUnit { get; set; }

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

            PartitionKey = $"{qboxMeasurement.ProductNumber}:{qboxMeasurement.SerialNumber}";
            RowKey = $"{qboxMeasurement.LogTime.Ticks}:{qboxMeasurement.CounterId:D4}";

            CorrelationId = qboxMeasurement.CorrelationId;
            SerialNumber = qboxMeasurement.SerialNumber;
            ProductNumber = qboxMeasurement.ProductNumber;
            LogTime = qboxMeasurement.LogTime;
            MeasureTime = qboxMeasurement.MeasureTime;
            CounterId = qboxMeasurement.CounterId;
            PulseValue = qboxMeasurement.PulseValue;
            PulsesPerUnit = (double)qboxMeasurement.PulsesPerUnit;
        }
    }
}
