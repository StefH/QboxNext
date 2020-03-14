using WindowsAzure.Table.Attributes;

namespace QBoxNext.Server.FunctionApp.Models
{
    /// <summary>
    /// Simple version based on QboxNext.Server.Infrastructure.Azure.Models.Internal.StateEntity
    /// </summary>
    public class StateEntity
    {
        // Example : "13-xx-xxx-xxx"
        [PartitionKey]
        public string SerialNumber { get; set; }

        // Example: "2518211341772170473"
        [RowKey]
        public string RowKey { get; set; }
    }
}