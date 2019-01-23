using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace NLog.Extensions.AzureTables
{
    /// <summary>
    /// Azure Table Storage NLog Target
    /// </summary>
    /// <seealso cref="TargetWithLayout" />
    [Target("AzureTableStorage")]
    public sealed class TableStorageTarget : TargetWithLayout
    {
        private readonly AzureStorageNameCache _containerNameCache = new AzureStorageNameCache();
        private readonly Func<string, string> _checkAndRepairTableNameDelegate;

        private CloudTableClient _client;
        private CloudTable _table;
        private string _machineName;

        // Delegates for bucket sorting
        private SortHelpers.KeySelector<AsyncLogEventInfo, TablePartitionKey> _getTablePartitionNameDelegate;

        [PublicAPI]
        [NotNull]
        [RequiredParameter]
        public string ConnectionString
        {
            get => _connectionString;

            set
            {
                _connectionString = value;
                CreateClient();
            }
        }
        private string _connectionString;

        [PublicAPI]
        [RequiredParameter]
        public Layout TableName { get; set; }

        [PublicAPI]
        [CanBeNull]
        public Layout CorrelationId { get; set; }

        [PublicAPI]
        public bool IncludeStackTrace { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageTarget"/> class.
        /// </summary>
        /// <remarks>The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code></remarks>
        public TableStorageTarget()
        {
            OptimizeBufferReuse = true;
            _checkAndRepairTableNameDelegate = CheckAndRepairTableNamingRules;
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            _machineName = GetMachineName();

            CreateClient();
        }

        private void CreateClient()
        {
            try
            {
                _client = CloudStorageAccount.Parse(_connectionString).CreateCloudTableClient();
                InternalLogger.Trace("AzureTableStorageTarget - Initialized");
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AzureTableStorageTarget(Name={0}): Failed to create TableClient with connectionString={1}.", Name, _connectionString);
                throw;
            }
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(logEvent.Message))
            {
                return;
            }

            var tableName = RenderLogEvent(TableName, logEvent);
            try
            {
                tableName = CheckAndRepairTableName(tableName);

                InitializeTable(tableName);

                string correlationId = CorrelationId != null ? RenderLogEvent(CorrelationId, logEvent) : null;
                string layoutMessage = RenderLogEvent(Layout, logEvent);
                var entity = CreateEntity(logEvent, layoutMessage, _machineName, logEvent.LoggerName, correlationId);
                var insertOperation = TableOperation.Insert(entity);
                TableExecute(_table, insertOperation);
            }
            catch (StorageException ex)
            {
                InternalLogger.Error(ex, "AzureTableStorage: failed writing to table: {0}", tableName);
                throw;
            }
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count <= 1)
            {
                base.Write(logEvents);
                return;
            }

            //must sort into containers and then into the blobs for the container
            if (_getTablePartitionNameDelegate == null)
            {
                _getTablePartitionNameDelegate = c => new TablePartitionKey(RenderLogEvent(TableName, c.LogEvent), c.LogEvent.LoggerName ?? string.Empty);
            }

            var partitionBuckets = SortHelpers.BucketSort(logEvents, _getTablePartitionNameDelegate);

            //Iterate over all the tables being written to
            foreach (var partitionBucket in partitionBuckets)
            {
                var tableName = partitionBucket.Key.TableName;

                try
                {
                    tableName = CheckAndRepairTableName(tableName);

                    InitializeTable(tableName);

                    //iterate over all the partition keys or we will get a System.ArgumentException: 'All entities in a given batch must have the same partition key.'
                    var batch = new TableBatchOperation();
                    //add each message for the destination table partition limit batch to 100 elements
                    foreach (var asyncLogEventInfo in partitionBucket.Value)
                    {
                        string correlationId = CorrelationId != null ? RenderLogEvent(CorrelationId, asyncLogEventInfo.LogEvent) : null;
                        string layoutMessage = RenderLogEvent(Layout, asyncLogEventInfo.LogEvent);
                        var entity = CreateEntity(asyncLogEventInfo.LogEvent, layoutMessage, _machineName, partitionBucket.Key.PartitionId, correlationId);
                        batch.Insert(entity);
                        if (batch.Count == 100)
                        {
                            TableExecuteBatch(_table, batch);
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        TableExecuteBatch(_table, batch);
                    }

                    foreach (var asyncLogEventInfo in partitionBucket.Value)
                    {
                        asyncLogEventInfo.Continuation(null);
                    }
                }
                catch (StorageException ex)
                {
                    InternalLogger.Error(ex, "AzureTableStorage: failed writing batch to table: {0}", tableName);
                    throw;
                }
            }
        }

        private static void TableExecute(CloudTable cloudTable, TableOperation insertOperation)
        {
            cloudTable.ExecuteAsync(insertOperation).GetAwaiter().GetResult();
        }

        private static void TableExecuteBatch(CloudTable cloudTable, TableBatchOperation batch)
        {
            cloudTable.ExecuteBatchAsync(batch).GetAwaiter().GetResult();
        }

        private void TableCreateIfNotExists(CloudTable cloudTable)
        {
            cloudTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes the Azure storage table and creates it if it doesn't exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        private void InitializeTable(string tableName)
        {
            if (_table == null || _table.Name != tableName)
            {
                _table = _client.GetTableReference(tableName);
                try
                {
                    TableCreateIfNotExists(_table);
                }
                catch (StorageException storageException)
                {
                    InternalLogger.Error(storageException, "AzureTableStorage: failed to get a reference to storage table.");
                    throw;
                }
            }
        }

        private string CheckAndRepairTableName(string tableName)
        {
            return _containerNameCache.LookupStorageName(tableName, _checkAndRepairTableNameDelegate);
        }

        private string CheckAndRepairTableNamingRules(string tableName)
        {
            InternalLogger.Trace("AzureTableStorage(Name={0}): Requested Table Name: {1}", Name, tableName);
            string validTableName = AzureStorageNameCache.CheckAndRepairTableNamingRules(tableName);
            if (validTableName == tableName)
            {
                InternalLogger.Trace("AzureTableStorage(Name={0}): Using Table Name: {0}", Name, validTableName);
            }
            else
            {
                InternalLogger.Trace("AzureTableStorage(Name={0}): Using Cleaned Table name: {0}", Name, validTableName);
            }

            return validTableName;
        }

        /// <summary>
        /// Gets the machine name
        /// </summary>
        private static string GetMachineName()
        {
            return TryLookupValue(() => Environment.GetEnvironmentVariable("COMPUTERNAME"), "COMPUTERNAME")
                   ?? TryLookupValue(() => Environment.GetEnvironmentVariable("HOSTNAME"), "HOSTNAME")
                   ?? TryLookupValue(System.Net.Dns.GetHostName, "DnsHostName");
        }

        private static string TryLookupValue(Func<string> lookupFunc, string lookupType)
        {
            try
            {
                string lookupValue = lookupFunc()?.Trim();
                return string.IsNullOrEmpty(lookupValue) ? null : lookupValue;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "AzureTableStorage: Failed to lookup {0}", lookupType);
                return null;
            }
        }

        private DynamicTableEntity CreateEntity(LogEventInfo logEvent, string layoutMessage, string machineName, string partitionKey, [CanBeNull] string correlationId)
        {
            string id = correlationId ?? Guid.NewGuid().ToString();
            string rowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:d19}:{id}";

            string exceptionValue = null;
            string stackTraceValue = null;
            string innerExceptionValue = null;
            if (logEvent.Exception != null)
            {
                var exception = logEvent.Exception;
                var innerException = exception.InnerException;
                if (exception is AggregateException aggregateException)
                {
                    var innerExceptions = aggregateException.Flatten();
                    if (innerExceptions.InnerExceptions?.Count == 1)
                    {
                        exception = innerExceptions.InnerExceptions[0];
                        innerException = null;
                    }
                    else
                    {
                        innerException = innerExceptions;
                    }

                    // Handle all Exceptions
                    aggregateException.Handle(x => true);
                }

                exceptionValue = string.Concat(exception.Message, " - ", exception.GetType().ToString());
                stackTraceValue = exception.StackTrace;
                if (innerException != null)
                {
                    innerExceptionValue = innerException.ToString();
                }
            }

            var properties = new Dictionary<string, EntityProperty>();
            if (!string.IsNullOrEmpty(correlationId))
            {
                properties.Add("CorrelationId", new EntityProperty(correlationId));
            }

            properties.Add("LogTimeStamp", new EntityProperty(logEvent.TimeStamp));
            properties.Add("Level", new EntityProperty(logEvent.Level.Name));
            properties.Add("LoggerName", new EntityProperty(logEvent.LoggerName));
            properties.Add("Message", new EntityProperty(logEvent.Message));
            properties.Add("FullMessage", new EntityProperty(layoutMessage));
            properties.Add("Exception", new EntityProperty(exceptionValue));
            properties.Add("InnerException", new EntityProperty(innerExceptionValue));

            if (IncludeStackTrace)
            {
                properties.Add("StackTrace", new EntityProperty(stackTraceValue));
            }

            properties.Add("MachineName", new EntityProperty(machineName));

            return new DynamicTableEntity(partitionKey, rowKey, "*", properties);
        }
    }
}