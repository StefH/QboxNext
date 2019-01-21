using System;

namespace NLog.Extensions.AzureTables
{
    internal struct TablePartitionKey : IEquatable<TablePartitionKey>
    {
        public readonly string TableName;
        public readonly string PartitionId;

        public TablePartitionKey(string tableName, string partitionId)
        {
            TableName = tableName;
            PartitionId = partitionId;
        }

        public bool Equals(TablePartitionKey other)
        {
            return TableName == other.TableName && PartitionId == other.PartitionId;
        }

        public override bool Equals(object obj)
        {
            return obj is TablePartitionKey key && Equals(key);
        }

        public override int GetHashCode()
        {
            return TableName.GetHashCode() ^ PartitionId.GetHashCode();
        }
    }
}