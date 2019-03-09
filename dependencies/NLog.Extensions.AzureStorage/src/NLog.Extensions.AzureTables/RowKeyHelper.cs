using System;

namespace NLog.Extensions.AzureTables
{
    internal static class RowKeyHelper
    {
        private static readonly long MaxTicks = DateTime.MaxValue.Ticks + 1;

        public static string Construct(DateTime timestamp)
        {
            return $"{MaxTicks - timestamp.Ticks:d19}";
        }
    }
}