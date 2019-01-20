using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Extensions
{
    public static class ParserServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IParserFactory" /> which can be used to get <see cref="MiniParser" />s from.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddParsers(this IServiceCollection services)
        {
            services.TryAddSingleton<IParserFactory, DefaultParserFactory>();
            services.TryAddTransient<IParserMatcher, DefaultParserMatcher>();

            // This could be moved elsewhere if extensibility is needed (f.ex. strategy pattern, action builder, or external config).
            services
                .AddParser(new ParserInfo { Type = typeof(MiniR07), MaxProtocolVersion = 0x02 })
                .AddParser(new ParserInfo { Type = typeof(MiniR16), MaxProtocolVersion = 0x27 })
                .AddParser(new ParserInfo { Type = typeof(MiniR21), MaxProtocolVersion = 0x29 })
                .AddParser(new ParserInfo { Type = typeof(MiniResponse), MaxProtocolVersion = -1 });

            return services;
        }

        private static IServiceCollection AddParser(this IServiceCollection services, ParserInfo parserInfo)
        {
            services
                .AddTransient(parserInfo.Type)
                .AddSingleton(parserInfo);

            return services;
        }
    }
}
