using System;
using System.Runtime.Serialization;

namespace QboxNext.Server.Domain
{
    [DataContract]
    public class QboxCounterData
    {
        [DataMember(Order = 1)]
        public string LabelText { get; set; }

        [DataMember(Order = 2)]
        public int LabelValue { get; set; }

        [DataMember(Order = 3)]
        public DateTime MeasureTime { get; set; }

        [DataMember(Order = 4)]
        public int Delta0181 { get; set; }

        [DataMember(Order = 5)]
        public int Delta0182 { get; set; }

        [DataMember(Order = 6)]
        public int Delta0281 { get; set; }

        [DataMember(Order = 7)]
        public int Delta0282 { get; set; }

        [DataMember(Order = 8)]
        public int Delta2421 { get; set; }

        [DataMember(Order = 9)]
        public QboxDataQuery DrillDownQuery { get; set; }
    }
}