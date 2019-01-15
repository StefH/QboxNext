using QboxNext.Qserver.Core.Model;

namespace QBoxNext.Business.Interfaces.Internal
{
    interface IMiniPocoFactory
    {
        /// <summary>
        /// Creates a MiniPoco.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="productNumber">The product number.</param>
        /// <returns>{MiniPoco}</returns>
        MiniPoco Create(string serialNumber, string productNumber);
    }
}