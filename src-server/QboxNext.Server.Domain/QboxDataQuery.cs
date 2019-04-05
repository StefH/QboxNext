using System;
using System.ComponentModel.DataAnnotations;

namespace QboxNext.Server.Domain
{
    public class QboxDataQuery
    {
        [Required]
        public DateTime From { get; set; }

        [Required]
        public DateTime To { get; set; }

        [Required]
        public QboxQueryResolution Resolution { get; set; }

        [Required]
        public bool AdjustHours { get; set; }
    }
}