using System;

namespace QboxNext.Server.Infrastructure.Azure.Utils
{
    internal static class PartitionKeyHelper
    {
        private const int PartitionKeyStart = 100000000;

        public static string Construct(string serialNumber, DateTime measureTime)
        {
            return $"{serialNumber}:{ConstructDateTimeKey(measureTime)}";
        }

        public static (string SerialNumber, DateTime MeasureTime) Deconstruct(string partitionKey)
        {
            var parts = partitionKey.Split(':');

            int value = PartitionKeyStart - int.Parse(parts[1]);
            int year = value / 10000;
            int month = (value - year * 10000) / 100;
            int day = value - year * 10000 - month * 100;

            return (parts[0], new DateTime(year, month, day));
        }

        private static int ConstructDateTimeKey(DateTime measureTime)
        {
            return PartitionKeyStart - measureTime.Year * 10000 - measureTime.Month * 100 - measureTime.Day;
        }
    }
}