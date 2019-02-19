using System.Linq;
using System.Reflection;

namespace QboxNext.Server.Common.Utils
{
    public static class AssemblyUtils
    {
        public static object GetVersion()
        {
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes().ToArray();
            var copyright = (AssemblyCopyrightAttribute)attributes.First(a => a is AssemblyCopyrightAttribute);
            var version = (AssemblyInformationalVersionAttribute)attributes.First(a => a is AssemblyInformationalVersionAttribute);

            return new
            {
                copyright.Copyright,
                version.InformationalVersion
            };
        }
    }
}