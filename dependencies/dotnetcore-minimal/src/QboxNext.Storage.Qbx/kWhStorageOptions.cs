namespace QboxNext.Storage.Qbx
{
    // ReSharper disable once InconsistentNaming
    public class kWhStorageOptions
    {
        /// <summary>
        /// Gets or sets the file name extension.
        /// </summary>
        public string Extension { get; set; } = ".qbx";

        /// <summary>
        /// The storage path.
        /// </summary>
        public string DataStorePath { get; set; }

        /// <summary>
        /// Gets or sets the number of days to grow a file when initializing or expanded.
        /// </summary>
        public int GrowthNrOfDays { get; set; } = 7;
    }
}
