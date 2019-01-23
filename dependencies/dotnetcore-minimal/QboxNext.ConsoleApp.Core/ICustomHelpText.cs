namespace QboxNext.ConsoleApp
{
    /// <summary>
    /// Describes a method of retrieving custom help text. Implement on command line options classes to override the help text.
    /// </summary>
    public interface ICustomHelpText
    {
        string GetHelpText(string defaultText);
    }
}