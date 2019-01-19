using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Common.Validation;
using System;

namespace QboxNext.WebApi.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public StatusController([NotNull] ILogger<StatusController> logger)
        {
            Guard.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        [HttpGet("/status")]
        public string Get()
        {
            _logger.LogInformation("Get status");
            return $"QboxNext.WebApi is running @ {DateTime.UtcNow}";
        }
    }
}