﻿using System;
using System.Threading;
using System.Threading.Tasks;
using QboxNext.Frontend.Blazor.Client.Services;
using Count;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ProtoBuf.Grpc.Client;
using QboxNext.Frontend.Blazor.Shared;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Client.Pages
{
    public class Model
    {
        public string Token { get; set; }
    }

    public partial class Counter
    {
        [Inject]
        public GrpcChannel Channel { get; set; }

        private int currentCount;
        private CancellationTokenSource cts;
        public Model Model = new Model();
        private string Error;

        [Inject]
        public GrpcBearerTokenProvider GrpcBearerTokenProvider { get; set; }

        private async Task IncrementCount()
        {
            cts = new CancellationTokenSource();

            try
            {
                Model.Token = await GrpcBearerTokenProvider.GetTokenAsync();
            }
            catch (AccessTokenNotAvailableException a)
            {
                a.Redirect();
            }

            var headers = new Metadata
            {
                { "Authorization", $"Bearer {Model.Token}" }
            };

            var query = Channel.CreateGrpcService<IDataQueryServiceContract>();
            var options = new CallOptions(headers);

            var q = new QboxDataQuery
            {
                From = DateTime.Now.AddDays(-1),
                To = DateTime.Now.AddDays(0),
                Resolution = QboxQueryResolution.Hour,
                AdjustHours = true
            };

            var dataResult1 = await query.GetCounterDataAsync(q, options);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(dataResult1));

            var client = new Count.Counter.CounterClient(Channel);
            var call = client.StartCounter(new CounterRequest { Start = currentCount }, headers, cancellationToken: cts.Token);

            try
            {
                Error = string.Empty;
                await foreach (var message in call.ResponseStream.ReadAllAsync())
                {
                    currentCount = message.Count;
                    StateHasChanged();
                }
            }
            catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.Cancelled)
            {
                // Ignore exception from cancellation
                Error = rpcException.Message;
            }
            catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.Unauthenticated)
            {
                Error = rpcException.Message;

            }
            catch (Exception exception)
            {
                Error = exception.Message;
            }
        }

        private void StopCount()
        {
            cts?.Cancel();
            cts = null;
        }
    }
}
