using System;

namespace QboxNext.Storage
{
    public struct StorageId
    {
        public static readonly StorageId Empty = new StorageId(string.Empty);

        private string _value;
        private string Value
        {
            get => _value ?? (_value = string.Empty);
        }

        private StorageId(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator StorageId(string value)
        {
            return new StorageId(value);
        }

        public static bool operator ==(StorageId first, StorageId second)
        {
            return Equals(first, second);
        }

        public static bool operator !=(StorageId first, StorageId second)
        {
            return !Equals(first, second);
        }

        public bool Equals(StorageId other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is StorageId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}