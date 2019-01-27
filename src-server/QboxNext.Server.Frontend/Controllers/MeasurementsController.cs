using System.IO;
using System.Threading.Tasks;
using CorrelationId;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Server.Common.Validation;
using QBoxNext.Server.Business.Interfaces.Public;

namespace QboxNext.Frontend.WebApi.Controllers
{
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ICorrelationContextAccessor _correlationContext;
        private readonly ILogger<MeasurementsController> _logger;

        public MeasurementsController(
            [NotNull] IRegistrationService registrationService,
            [NotNull] ICorrelationContextAccessor correlationContext,
            [NotNull] ILogger<MeasurementsController> logger)
        {
            Guard.NotNull(registrationService, nameof(registrationService));
            Guard.NotNull(correlationContext, nameof(correlationContext));
            Guard.NotNull(logger, nameof(logger));

            _registrationService = registrationService;
            _correlationContext = correlationContext;
            _logger = logger;
        }

        [HttpGet(@"/device/{serialNumber:regex(^\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}$)}")]
        public async Task<ActionResult> PostAsync([NotNull] string serialNumber)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            if (!await _registrationService.IsValidRegistrationAsync(serialNumber))
            {
                return BadRequest();
            }

            return Ok("!");
        }
    }
}
