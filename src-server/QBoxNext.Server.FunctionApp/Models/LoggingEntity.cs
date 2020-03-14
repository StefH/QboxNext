using WindowsAzure.Table.Attributes;

namespace QBoxNext.Server.FunctionApp.Models
{
    /// <summary>
    /// Simple version from the LoggingEntry as defined in NLog.Extensions.AzureTables.LoggingEntity
    /// </summary>
    public class LoggingEntity
    {
        [PartitionKey]
        public string PartitionKey { get; set; }

        [RowKey]
        public string RowKey { get; set; }
    }
}