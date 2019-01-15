using System.ComponentModel.DataAnnotations;

namespace QboxNext.Infrastructure.Azure.Options
{
    public class AzureTableStorageOptions
    {
        [Required]
        [MinLength(0)]
        public string ConnectionString { get; set; }

        [Required]
        [MinLength(0)]
        public string MeasurementsTableName { get; set; }

        [Required]
        [Range(0, 3600)]
        public int ServerTimeout { get; set; }
    }
}