using System.ComponentModel.DataAnnotations;

namespace QBoxNext.Server.FunctionApp.Options
{
    public class AzureTableStorageCleanerOptions
    {
        [Required]
        [MinLength(0)]
        public string ConnectionString { get; set; }

        [Required]
        [MinLength(0)]
        public string LoggingTableName { get; set; }

        [Required]
        public int LoggingTableRetentionInMonths { get; set; }

        public bool LoggingTableDeleteRows { get; set; }

        [Required]
        [MinLength(0)]
        public string StatesTableName { get; set; }

        [Required]
        public int StatesTableRetentionInMonths { get; set; }

        public bool StatesTableDeleteRows { get; set; }

        [Required]
        [Range(0, 3600)]
        public int ServerTimeout { get; set; }
    }
}