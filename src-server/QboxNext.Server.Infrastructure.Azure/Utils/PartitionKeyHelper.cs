using System;

namespace QboxNext.Server.Infrastructure.Azure.Utils
{
    internal static class PartitionKeyHelper
    {
        private const int PartitionKeyStart = 100000000;

        public static string GetPartitionKey(string serialNumber, DateTime measureTime)
        {
            return $"{serialNumber}:{GetDateTimeKey(measureTime)}";
        }

        public static int GetDateTimeKey(DateTime measureTime)
        {
            return PartitionKeyStart - measureTime.Year * 10000 - measureTime.Month * 100 - measureTime.Day;
        }
    }
}