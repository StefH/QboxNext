using System.Threading.Tasks;

namespace QBoxNext.Server.FunctionApp.Services
{
    public interface IAzureTableStorageCleaner
    {
        Task CleanupStatesAsync();

        Task CleanupLoggingAsync();
    }
}