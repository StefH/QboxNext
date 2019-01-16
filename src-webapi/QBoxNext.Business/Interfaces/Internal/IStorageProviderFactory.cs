namespace QBoxNext.Business.Interfaces.Internal
{
    internal interface IStorageProviderFactory
    {
        /// <summary>
        /// Creates IStorageProvider.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="productNumber">The product number.</param>
        /// <param name="counterId">The counter identifier.</param>
        /// <returns>IStorageProviderAsync</returns>
        IStorageProviderAsync Create(string serialNumber, string productNumber, int counterId);
    }
}