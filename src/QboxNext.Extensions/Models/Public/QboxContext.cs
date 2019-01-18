using JetBrains.Annotations;

namespace QboxNext.Extensions.Models.Public
{
    public class QboxContext
    {
        [CanBeNull]
        public string LastSeenAtUrl { get; set; }

        [CanBeNull]
        public string ExternalIp { get; set; }

        [NotNull]
        public string ProductNumber { get; set; }

        [NotNull]
        public string SerialNumber { get; set; }

        public byte[] Message { get; set; }
    }
}
