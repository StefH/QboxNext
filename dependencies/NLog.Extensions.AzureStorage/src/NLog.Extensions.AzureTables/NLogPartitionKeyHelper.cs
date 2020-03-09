using System;

namespace NLog.Extensions.AzureTables
{
    internal static class NLogPartitionKeyHelper
    {
        private const int PartitionKeyStart = 100000000;

        public static string Construct(DateTime measureTime)
        {
            return $"{PartitionKeyStart - measureTime.Year * 10000 - measureTime.Month * 100 - measureTime.Day}";
        }

        public static DateTime? Deconstruct(string partitionKey)
        {
            if (string.IsNullOrEmpty(partitionKey))
            {
                return null;
            }

            int value = PartitionKeyStart - int.Parse(partitionKey);
            int year = value / 10000;
            int month = (value - year * 10000) / 100;
            int day = value - year * 10000 - month * 100;

            return new DateTime(year, month, day);
        }
    }
}