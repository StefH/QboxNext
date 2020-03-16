using System;
using QboxNext.Core.Interfaces;

namespace QboxNext.Core.Utils
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
        
        public DateTime Now => DateTime.Now;
    }
}