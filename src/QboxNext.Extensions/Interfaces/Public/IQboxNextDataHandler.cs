using System.Threading.Tasks;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IQboxNextDataHandler : IVisitor
    {
        /// <summary>
        /// Handles the request message.
        /// </summary>
        /// <returns>The response message.</returns>
        Task<string> HandleAsync();
    }
}