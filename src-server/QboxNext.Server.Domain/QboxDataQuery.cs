using System;
using System.ComponentModel.DataAnnotations;

namespace QboxNext.Server.Domain
{
    public class QboxDataQuery
    {
        [Required]
        //[RegularExpression(@"\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}")]
        public string SerialNumber { get; set; }

        public int[] CounterIds { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public QueryResolution Resolution { get; set; }
    }
}