using System;

namespace QboxNext.Server.Domain
{
    public class QboxCounterDataValue
    {
        public string LabelText { get; set; }

        public int LabelValue { get; set; }

        public DateTime MeasureTime { get; set; }

        //public int CounterId { get; set; }

        //public int AveragePulseValue { get; set; }

        //public int Min { get; set; }

        //public int Max { get; set; }

        //public int Delta { get; set; }

        public int? Delta0181 { get; set; }

        public int? Delta0182 { get; set; }

        public int? Delta0281 { get; set; }

        public int? Delta0282 { get; set; }

        public int? Delta2421 { get; set; }
    }
}