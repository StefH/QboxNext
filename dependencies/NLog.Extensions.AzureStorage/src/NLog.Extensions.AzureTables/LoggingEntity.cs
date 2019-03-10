using System;
using WindowsAzure.Table.Attributes;

namespace NLog.Extensions.AzureTables
{
    public class LoggingEntity
    {
        [PartitionKey]
        public string PartitionKey { get; set; }

        [RowKey]
        public string RowKey { get; set; }

        public string CorrelationId { get; set; }

        public DateTime LogTimeStamp { get; set; }

        public string MachineName { get; set; }

        public string Level { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public string FullMessage { get; set; }

        public string Exception { get; set; }

        public string InnerException { get; set; }

        public string StackTrace { get; set; }
    }
}