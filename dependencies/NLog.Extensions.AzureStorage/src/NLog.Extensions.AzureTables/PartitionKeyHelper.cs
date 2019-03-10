using System;

namespace NLog.Extensions.AzureTables
{
    internal static class PartitionKeyHelper
    {
        private const int PartitionKeyStart = 100000000;

        public static string Construct(DateTime measureTime)
        {
            return $"{PartitionKeyStart - measureTime.Year * 10000 - measureTime.Month * 100 - measureTime.Day}";
        }
    }
}