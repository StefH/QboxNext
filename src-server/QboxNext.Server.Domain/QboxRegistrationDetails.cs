namespace QboxNext.Server.Domain
{
    public class QboxRegistrationDetails
    {
        public bool IsRegistered { get; set; }

        public bool FirmwareDownloadAllowed { get; set; }

        public string Firmware { get; set; }
    }
}