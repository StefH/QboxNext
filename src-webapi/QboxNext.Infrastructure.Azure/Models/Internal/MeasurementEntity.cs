using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Core.Utils;
using QboxNext.Domain;
using System;
using CommandLine.Text;

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
        /// <param name="measurement">The measurement.</param>
        public MeasurementEntity([NotNull] Measurement measurement)
        {
            Guard.IsNotNull(measurement, nameof(measurement));

            PartitionKey = $"{measurement.ProductNumber}:{measurement.SerialNumber}";
            RowKey = $"{measurement.CounterId:D4}:{measurement.LogTime.Ticks}";

            CorrelationId = measurement.CorrelationId;
            SerialNumber = measurement.SerialNumber;
            ProductNumber = measurement.ProductNumber;
            LogTime = measurement.LogTime;
            MeasureTime = measurement.MeasureTime;
            CounterId = measurement.CounterId;
            PulseValue = measurement.PulseValue;
            PulsesPerUnit = (double)measurement.PulsesPerUnit;
        }
    }
}
