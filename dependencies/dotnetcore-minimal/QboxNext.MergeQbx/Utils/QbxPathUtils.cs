using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace QboxNext.MergeQbx.Utils
{
    public static class QbxPathUtils
    {
        public static string GetSerialFromPath(string qbxPath)
        {
            var regex = new Regex(@"\d\d-\d\d-\d\d\d-\d\d\d");
            var match = regex.Match(qbxPath);
            if (match.Success)
            {
                return match.Value;
            }
            throw new InvalidDataException($"Could not parse serial from path ${qbxPath}");
        }

        public static string GetBaseDirFromPath(string qbxPath)
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(qbxPath));
        }

        public static int GetCounterIdFromPath(string qbxPath)
        {
            var regex = new Regex(@"_(\d{8})(_|\.)");
            var match = regex.Match(qbxPath);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            throw new InvalidDataException($"Could not parse counter ID from path ${qbxPath}");
        }

        public static string GetStorageIdFromPath(string qbxPath)
        {
            var regex = new Regex(@"(\d\d-\d\d-\d\d\d-\d\d\d_\d{8}_.+)\.qbx");
            var match = regex.Match(qbxPath);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
    }
}
