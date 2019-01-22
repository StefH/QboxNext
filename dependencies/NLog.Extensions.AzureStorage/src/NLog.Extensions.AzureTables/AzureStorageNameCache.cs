using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLog.Extensions.AzureTables
{
    internal sealed class AzureStorageNameCache
    {
        private const string TrimLeadingPattern = "^.*?(?=[a-zA-Z])";
        private const string TrimForbiddenCharactersPattern = "[^a-zA-Z0-9-]";

        private readonly Dictionary<string, string> _storageNameCache = new Dictionary<string, string>();

        public string LookupStorageName(string requestedName, Func<string, string> checkAndRepairName)
        {
            if (_storageNameCache.TryGetValue(requestedName, out var validName))
            {
                return validName;
            }

            if (_storageNameCache.Count > 1000)
            {
                _storageNameCache.Clear();
            }

            validName = checkAndRepairName(requestedName);
            _storageNameCache[requestedName] = validName;
            return validName;
        }

        private static string EnsureValidName(string name, bool ensureToLower = false)
        {
            if (name?.Length > 0)
            {
                if (char.IsWhiteSpace(name[0]) || char.IsWhiteSpace(name[name.Length - 1]))
                {
                    name = name.Trim();
                }

                for (int i = 0; i < name.Length; ++i)
                {
                    char chr = name[i];
                    if (chr >= 'A' && chr <= 'Z')
                    {
                        if (ensureToLower)
                        {
                            name = name.ToLowerInvariant();
                        }
                        continue;
                    }

                    if (chr >= 'a' && chr <= 'z')
                    {
                        continue;
                    }

                    if (i != 0 && chr >= '0' && chr <= '9')
                    {
                        continue;
                    }

                    return null;
                }

                return name;
            }

            return null;
        }

        /// <summary>
        /// Checks the and repairs table name according to the Azure naming rules.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static string CheckAndRepairTableNamingRules([NotNull] string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException(tableName, nameof(tableName));
            }

            /*  http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx
            Table Names:
                Table names must be unique within an account.
                Table names may contain only alphanumeric characters.
                Table names cannot begin with a numeric character.
                Table names are case-insensitive.
                Table names must be from 3 to 63 characters long.
                Some table names are reserved, including "tables". Attempting to create a table with a reserved table name returns error code 404 (Bad Request).
            */
            string simpleValidName = tableName.Length <= 63 ? EnsureValidName(tableName) : null;
            if (simpleValidName?.Length >= 3)
            {
                return simpleValidName;
            }

            string pass1 = Regex.Replace(tableName, TrimForbiddenCharactersPattern, string.Empty, RegexOptions.None);
            string cleanedTableName = Regex.Replace(pass1, TrimLeadingPattern, string.Empty, RegexOptions.None);
            if (string.IsNullOrWhiteSpace(cleanedTableName) || cleanedTableName.Length > 63 || cleanedTableName.Length < 3)
            {
                return "Logs";
            }

            return tableName;
        }
    }
}
