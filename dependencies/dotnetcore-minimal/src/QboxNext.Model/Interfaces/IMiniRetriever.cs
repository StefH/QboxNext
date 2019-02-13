using QboxNext.Model.Qboxes;

namespace QboxNext.Model.Interfaces
{
    /// <summary>
    /// Interface for retrieving Mini objects.
    /// </summary>
    public interface IMiniRetriever
    {
        Mini Retrieve(string qboxSerial);
    }
}
