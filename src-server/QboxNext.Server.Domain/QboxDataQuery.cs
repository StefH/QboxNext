using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace QboxNext.Server.Domain
{
    [DataContract]
    public class QboxDataQuery
    {
        [Required]
        [DataMember(Order = 1)]
        public DateTime From { get; set; }

        [Required]
        [DataMember(Order = 2)]
        public DateTime To { get; set; }

        [Required]
        [DataMember(Order = 3)]
        public QboxQueryResolution Resolution { get; set; }

        [Required]
        [DataMember(Order = 4)]
        public bool AdjustHours { get; set; }
    }
}