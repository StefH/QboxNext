using JetBrains.Annotations;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Interfaces.Public
{
    public interface IRegistrationService
    {
        /// <summary>
        /// Checks if the SerialNumber is a valid registration.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns>true/false</returns>
        Task<bool> IsValidRegistrationAsync([CanBeNull] string serialNumber);
    }
}