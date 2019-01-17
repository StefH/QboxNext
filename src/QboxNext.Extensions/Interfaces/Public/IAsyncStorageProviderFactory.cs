namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IAsyncStorageProviderFactory
    {
        /// <summary>
        /// Creates <see cref="IAsyncStorageProvider"/>.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="productNumber">The product number.</param>
        /// <param name="counterId">The counter identifier.</param>
        /// <returns>IStorageProviderAsync</returns>
        IAsyncStorageProvider Create(string serialNumber, string productNumber, int counterId);
    }
}