using JetBrains.Annotations;
using System.Threading.Tasks;
using QboxNext.Server.Domain;

namespace QBoxNext.Server.Business.Interfaces.Public
{
    public interface IRegistrationService
    {
        /// <summary>
        /// Get QboxRegistrationDetails for a SerialNumber.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns>QboxRegistrationDetails if found, else null</returns>
        Task<QboxRegistrationDetails> GetQboxRegistrationDetailsAsync([CanBeNull] string serialNumber);
    }
}