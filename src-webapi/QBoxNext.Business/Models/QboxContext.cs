namespace QBoxNext.Business.Models
{
    public class QboxContext
    {
        public string LastSeenAtUrl { get; set; }

        public string ExternalIp { get; set; }

        public string ProductNumber { get; set; }

        public string SerialNumber { get; set; }

        public byte[] Message { get; set; }
    }
}
