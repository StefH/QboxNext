using System;
using QboxNext.Core.Extensions;

namespace QboxNext.Storage
{
    public class StorageProviderContext
    {
        private DateTime _referenceDate;

        public StorageProviderContext()
        {
            ReferenceDate = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the serial number for the Qbox that holds the counter.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the Qbox counter id.
        /// </summary>
        public int CounterId { get; set; }

        /// <summary>
        /// Gets or sets the precision to calculate the values when storing the data for the kWh and money fields.
        /// </summary>
        public Precision Precision { get; set; }

        /// <summary>
        /// StorageId added by firmware 39
        /// By StorageId = null/empty, the old filename is used
        /// By StorageId != null, value is used for filename
        /// </summary>
        //todo: refactor naar 1 storage id methode naam geving. Let wel op dat dan een tool geschreven moet worden om alle bestaande files eerst te hernoemen
        public StorageId StorageId { get; set; }

        /// <summary>
        /// Property that signals that it is allowed to overwrite given values.
        /// In normal operation it should not be allowed to overwrite values stored because the Qbox sends its values every minute
        /// and if a value in the past is send (twice) this should be seen as an error. The time in the qbox can be off due to hardware
        /// choices. That would allow overwrite of faulty values over existing correct values.
        /// To allow the recalculation of a file with its values for kwh and money it is in extraordinary cases needed to allow for overwrites.
        /// </summary>
        /// <remarks>
        /// Should probably be moved elsewhere.
        /// </remarks>
        public bool AllowOverwrite { get; set; }

        /// <summary>
        /// Gets or sets the reference datetime in whole minutes.
        /// </summary>
        public DateTime ReferenceDate
        {
            get => _referenceDate;
            set => _referenceDate = value.TruncateToMinute();
        }
    }
}
