using QboxNext.Model.Qboxes;

namespace QboxNext.Extensions.Interfaces.Internal
{
    interface IQboxMiniFactory
    {
        /// <summary>
        /// Creates a <see cref="Mini"/>.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <returns><see cref="Mini"/></returns>
        Mini Create(string serialNumber);
    }
}