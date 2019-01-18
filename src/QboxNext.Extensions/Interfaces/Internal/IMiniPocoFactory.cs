using QboxNext.Qserver.Core.Model;

namespace QboxNext.Extensions.Interfaces.Internal
{
    interface IMiniPocoFactory
    {
        /// <summary>
        /// Creates a <see cref="MiniPoco"/>.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="productNumber">The product number.</param>
        /// <returns><see cref="MiniPoco"/></returns>
        MiniPoco Create(string serialNumber, string productNumber);
    }
}