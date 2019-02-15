using System;

namespace QboxNext.Server.Infrastructure.Azure.Utils
{
    internal static class PartitionKeyHelper
    {
        private const int PartitionKeyStart = 100000000;

        public static string ConstructPartitionKey(string serialNumber, DateTime measureTime)
        {
            return $"{serialNumber}:{ConstructDateTimeKey(measureTime)}";
        }

        private static int ConstructDateTimeKey(DateTime measureTime)
        {
            return PartitionKeyStart - measureTime.Year * 10000 - measureTime.Month * 100 - measureTime.Day;
        }
    }
}