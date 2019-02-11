using System;

namespace QboxNext.Model.Qboxes
{
    public class CounterDeviceMapping
    {
        public DateTime PeriodeEind { get; set; }
        public DateTime PeriodeBegin { get; set; }
        public Device Device { get; set; }
    }
}