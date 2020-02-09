using System;

namespace QboxNext.Server.Infrastructure.Azure.Utils
{
    internal static class DateTimeRowKeyHelper
    {
        private static readonly long MaxTicks = DateTime.MaxValue.Ticks + 1;

        public static string Construct(DateTime measureTime)
        {
            return $"{MaxTicks - measureTime.Ticks:d19}";
        }

        public static DateTime? Deconstruct(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey))
            {
                return null;
            }

            var value = long.Parse(rowKey);
            return new DateTime(MaxTicks - value);
        }
    }
}