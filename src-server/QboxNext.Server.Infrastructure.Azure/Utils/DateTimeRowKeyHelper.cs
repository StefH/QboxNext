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
    }
}