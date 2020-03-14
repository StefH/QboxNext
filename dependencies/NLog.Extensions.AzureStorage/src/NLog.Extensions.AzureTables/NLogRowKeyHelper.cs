using System;

namespace NLog.Extensions.AzureTables
{
    internal static class NLogRowKeyHelper
    {
        private static readonly long MaxTicks = DateTime.MaxValue.Ticks + 1;

        public static string Construct(DateTime timestamp)
        {
            return $"{MaxTicks - timestamp.Ticks:d19}";
        }

        public static DateTime? Deconstruct(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey))
            {
                return null;
            }

            long value = long.Parse(rowKey);
            return new DateTime(MaxTicks - value);
        }
    }
}