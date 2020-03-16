using System;

namespace QboxNext.Core.Interfaces
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }

        DateTime Now { get; }
    }
}