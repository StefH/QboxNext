using JetBrains.Annotations;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using QboxNext.Server.Common.Validation;

namespace QboxNext.Server.DataReceiver.Telemetry
{
    public class QboxNextTelemetryInitializer : ITelemetryInitializer
    {
        private static string PropertyNameCorrelationID = "CorrelationID";

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextTelemetryInitializer"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public QboxNextTelemetryInitializer([NotNull] IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            // Is this a TrackRequest() ?
            if (!(telemetry is RequestTelemetry requestTelemetry))
            {
                return;
            }

            requestTelemetry.Context.GlobalProperties[PropertyNameCorrelationID] = _httpContextAccessor.HttpContext.TraceIdentifier;
        }
    }
}