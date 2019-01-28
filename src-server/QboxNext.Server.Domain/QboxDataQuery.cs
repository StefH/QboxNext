using System;
using System.ComponentModel.DataAnnotations;

namespace QboxNext.Server.Domain
{
    public class QboxDataQuery
    {
        [Required]
        [RegularExpression(@"\d{2}-\d{2}-\d{3}-\d{3}")]
        public string SerialNumber { get; set; }

        [Required]
        public int[] CounterIds { get; set; }

        [Required]
        public DateTime From { get; set; }

        [Required]
        public DateTime To { get; set; }

        [Required]
        public QueryResolution Resolution { get; set; }
    }
}