using System;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using QboxNext.Frontend.Blazor.Shared;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Client.Services
{
    public class AuthenticatedDataQueryClient
    {
        private readonly GrpcBearerTokenProvider _tokenProvider;
        private readonly GrpcChannel _channel;

        public AuthenticatedDataQueryClient(GrpcBearerTokenProvider tokenProvider, GrpcChannel channel)
        {
            _tokenProvider = tokenProvider;
            _channel = channel;
        }

        public async Task<QboxPagedCounterDataResult> GetCounterDataAsync(QboxDataQuery query)
        {
            string token = await _tokenProvider.GetTokenAsync();

            var headers = new Metadata
            {
                { "Authorization", $"Bearer {token}" }
            };

            var client = _channel.CreateGrpcService<IDataQueryServiceContract>();
            var options = new CallOptions(headers);

            return await client.GetCounterDataAsync(query, options);
        }
    }
}
