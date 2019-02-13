﻿using System;

namespace QboxNext.Server.Infrastructure.Azure.Utils
{
    internal static class RowKeyHelper
    {
        private static readonly long MaxTicks = DateTime.MaxValue.Ticks + 1;

        public static string GetRowKey(DateTime measureTime)
        {
            return $"{MaxTicks - measureTime.Ticks:d19}";
        }
    }
}