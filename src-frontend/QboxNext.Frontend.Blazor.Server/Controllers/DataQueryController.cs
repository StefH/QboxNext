using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Security.Auth0.Authorization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QBoxNext.Server.Business.Interfaces.Public;

namespace QboxNext.Frontend.Blazor.Server.Controllers
{
    [Authorize(QboxNextPolicies.ReadData)]
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

        [HttpPost("/api/data")]
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> QueryAsync([NotNull, FromBody] QboxDataQuery query)
        {
            Guard.NotNull(query, nameof(query));

            var claimsIdentity = (Auth0ClaimsIdentity)User.Identities.First(i => i is Auth0ClaimsIdentity);
            string serialNumber = claimsIdentity.AppMetadata["qboxSerialNumber"].ToString();

            if (!await _registrationService.IsValidRegistrationAsync(serialNumber))
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation("Querying {SerialNumber} with {Query}", serialNumber, JsonConvert.SerializeObject(query));

            return await _dataQueryService.QueryAsync(serialNumber, query);
        }
    }
}