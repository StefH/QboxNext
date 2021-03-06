﻿using AspNetCore.Security.Auth0.Authorization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QBoxNext.Server.Business.Interfaces.Public;
using System.Linq;
using System.Threading.Tasks;

namespace QboxNext.Server.Frontend.Controllers
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
        public async Task<ActionResult> QueryAsync([NotNull, FromBody] QboxDataQuery query)
        {
            Guard.NotNull(query, nameof(query));

            var claimsIdentity = (Auth0ClaimsIdentity)User.Identities.First(i => i is Auth0ClaimsIdentity);
            string serialNumber = claimsIdentity.AppMetadata["qboxSerialNumber"].ToString();

            if (await _registrationService.GetQboxRegistrationDetailsAsync(serialNumber) == null)
            {
                return BadRequest();
            }

            _logger.LogInformation("Querying {SerialNumber} with {Query}", serialNumber, JsonConvert.SerializeObject(query));

            var result = await _dataQueryService.QueryAsync(serialNumber, query);

            return Ok(result);
        }
    }
}