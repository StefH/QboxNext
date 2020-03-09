using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using WindowsAzure.Table;

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

        private (string Name, ITableSet<LoggingEntity> Set) _table;

        [PublicAPI]
        [NotNull]
        public string MachineName { get; set; }

        [PublicAPI]
        [NotNull]
        [RequiredParameter]
        public string ConnectionString { get; set; }

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

            if (string.IsNullOrEmpty(MachineName))
            {
                MachineName = GetMachineName();
            }

            InitTable();
        }

        private void InitTable()
        {
            try
            {
                var client = CloudStorageAccount.Parse(ConnectionString).CreateCloudTableClient();

                string tableName = CheckAndRepairTableName(RenderLogEvent(TableName, LogEventInfo.CreateNullEvent()));

                _table = (tableName, new TableSet<LoggingEntity>(client, tableName));

                InternalLogger.Trace("AzureTableStorageTarget - Initialized");
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AzureTableStorageTarget(Name={0}): Failed to create TableClient with connectionString={1}.", Name, ConnectionString);
                throw;
            }
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(logEvent.Message))
            {
                return;
            }

            try
            {
                var entity = MapEntity(logEvent);

                _table.Set.AddOrUpdate(entity);
            }
            catch (StorageException ex)
            {
                InternalLogger.Error(ex, "AzureTableStorage: failed writing entry to table: {0}", _table.Name);
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
            try
            {
                var loggingEntities = logEvents.Select(l => l.LogEvent).Select(MapEntity);

                _table.Set.AddOrUpdate(loggingEntities);
            }
            catch (StorageException ex)
            {
                InternalLogger.Error(ex, "AzureTableStorage: failed writing entries to table: {0}", _table.Name);
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

        private LoggingEntity MapEntity(LogEventInfo logEvent)
        {
            var now = DateTime.UtcNow;
            string correlationId = CorrelationId != null ? RenderLogEvent(CorrelationId, logEvent) : null;
            string layoutMessage = RenderLogEvent(Layout, logEvent);
            string rowKey = NLogRowKeyHelper.Construct(now);
            string partitionKey = NLogPartitionKeyHelper.Construct(now);

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

            return new LoggingEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                CorrelationId = correlationId,
                Message = logEvent.Message,
                MachineName = MachineName,
                LogTimeStamp = logEvent.TimeStamp,
                Exception = exceptionValue,
                FullMessage = layoutMessage,
                InnerException = innerExceptionValue,
                Level = logEvent.Level.Name,
                LoggerName = logEvent.LoggerName,
                StackTrace = stackTraceValue
            };
        }
    }
}