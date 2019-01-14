
namespace QboxNext.Qserver.Classes
{
    /// <summary>
    /// FirmwareVersionMapper maps a Qbox serial number to the version of the firmware that that Qbox should use.
    /// </summary>
    public class FirmwareVersionMapper
    {
        public string FromSerialNumber(string sn)
        {
            // In Qmanager all firmwares in production are set to encrypt Qbox messages.
            // To make it easier to reverse engineer, we try to use the firmware without encryption.
            var firmwareVersion = "A47_ENCRYPT_OFF_svn_ver_680_P1_Debug";
            int weekId = GetWeekIdFromSerialNumber(sn);
            if (weekId < 1213)
            {
                firmwareVersion = "A14_Encrypt_Off_v325";
            }
            else if (weekId < 1245)
            {
                firmwareVersion = "A16_Encrypt_Off_v384";
            }
            else if (weekId < 1248)
            {
                firmwareVersion = "A37_Encrypt_Off_v516";
            }
            // SAM: In Qmanager all Qboxes between 12-48 and 15-19 (the bulk!) are using A46. 
            // I think A47 will work on those Qboxes as well. If not we have to uncomment the following lines.
            //else if (weekId < 1519)
            //{
            //    firmwareVersion = "A46 Encryption ON rev676";   // No firmware available with encryption off.
            //}

            return firmwareVersion;
        }

        private int GetWeekIdFromSerialNumber(string sn)
        {
            var snParts = sn.Split("-");
            var weekId = int.Parse(snParts[0]) * 100 + int.Parse(snParts[1]);
            return weekId;
        }
    }
}
