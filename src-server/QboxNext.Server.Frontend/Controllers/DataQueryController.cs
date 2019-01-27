using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QBoxNext.Server.Business.Interfaces.Public;
using System.Threading.Tasks;

namespace QboxNext.Server.Frontend.Controllers
{
    [ApiController]
    public class DataQueryController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IDataQueryService _dataQueryService;
        private readonly ILogger<DataQueryController> _logger;

        public DataQueryController(
            [NotNull] IRegistrationService registrationService,
            [NotNull] IDataQueryService dataQueryService,
            [NotNull] ILogger<DataQueryController> logger)
        {
            Guard.NotNull(registrationService, nameof(registrationService));
            Guard.NotNull(dataQueryService, nameof(dataQueryService));
            Guard.NotNull(logger, nameof(logger));

            _registrationService = registrationService;
            _dataQueryService = dataQueryService;
            _logger = logger;
        }

        [HttpPost(@"/data")]
        public async Task<ActionResult> QueryAsync([NotNull, FromBody] QboxDataQuery query)
        {
            Guard.NotNull(query, nameof(query));

            if (!await _registrationService.IsValidRegistrationAsync(query.SerialNumber))
            {
                return BadRequest();
            }

            _logger.LogInformation("Querying with {Query}", JsonConvert.SerializeObject(query));

            var result = await _dataQueryService.QueryAsync(query);

            return Ok(result);
        }
    }
}