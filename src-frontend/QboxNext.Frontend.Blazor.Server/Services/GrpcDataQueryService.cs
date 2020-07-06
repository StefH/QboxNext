using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.Security.Auth0.Authorization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using QboxNext.Frontend.Blazor.Shared;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QBoxNext.Server.Business.Interfaces.Public;

namespace QboxNext.Frontend.Blazor.Server.Services
{
    [Authorize(QboxNextPolicies.ReadData)]
    public class GrpcDataQueryService : IDataQueryServiceContract
    {
        private readonly IRegistrationService _registrationService;
        private readonly IDataQueryService _dataQueryService;
        private readonly ILogger<GrpcDataQueryService> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public GrpcDataQueryService(
            [NotNull] IHttpContextAccessor httpContextAccessor,
            [NotNull] IRegistrationService registrationService,
            [NotNull] IDataQueryService dataQueryService,
            [NotNull] ILogger<GrpcDataQueryService> logger)
        {
            Guard.NotNull(registrationService, nameof(registrationService));
            Guard.NotNull(dataQueryService, nameof(dataQueryService));
            Guard.NotNull(logger, nameof(logger));

            _httpContextAccessor = httpContextAccessor;
            _registrationService = registrationService;
            _dataQueryService = dataQueryService;
            _logger = logger;
        }

        public async Task<QboxPagedCounterDataResult> GetCounterDataAsync(QboxDataQuery request, CallContext context)
        {
            Guard.NotNull(request, nameof(request));

            string serialNumber;
            try
            {
                var claimsIdentity = (Auth0ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identities.First(i => i is Auth0ClaimsIdentity);
                serialNumber = claimsIdentity.AppMetadata[QboxAppMetadataConstants.QboxSerialNumber].ToString();
            }
            catch
            {
                throw new UnauthorizedAccessException($"No {nameof(Auth0ClaimsIdentity)} is present or {nameof(Auth0ClaimsIdentity)} does not contain {QboxAppMetadataConstants.QboxSerialNumber}.");
            }

            if (!await _registrationService.IsValidRegistrationAsync(serialNumber))
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation("Querying {SerialNumber} with {Query}", serialNumber, JsonSerializer.Serialize(request));

            var result = await _dataQueryService.QueryAsync(serialNumber, request);

            return new QboxPagedCounterDataResult
            {
                Count = result.Count,
                Overview = result.Overview,
                Items = result.Items
            };
        }
    }
}
