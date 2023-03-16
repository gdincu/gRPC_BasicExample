using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcGreeter
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<ExampleResponse> UnaryCall(ExampleRequest request,
    ServerCallContext context)
        {
            var response = new ExampleResponse { IsDescending = request.IsDescending, PageIndex = request.PageIndex+5, PageSize = request.PageSize+7 };
            return Task.FromResult(response);
        }

        public override async Task StreamingFromServer(ExampleRequest request,
    IServerStreamWriter<ExampleResponse> responseStream, ServerCallContext context)
        {
            int TempPageIndex = request.PageIndex;
            for (var i = 0; i < 5; i++)
            {
                TempPageIndex++;
                await responseStream.WriteAsync(new ExampleResponse { IsDescending = request.IsDescending, PageIndex = TempPageIndex, PageSize = request.PageSize+13 });
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

        }

        public override async Task<ExampleResponse> StreamingFromClient(
    IAsyncStreamReader<ExampleRequest> requestStream, ServerCallContext context)
        {   
            while (await requestStream.MoveNext())
            {       
                Console.WriteLine("StreamingFromClient -> PageIndex: " + requestStream.Current.PageIndex);
            }
            return new ExampleResponse();
        }

        public override async Task StreamingBothWays(IAsyncStreamReader<ExampleRequest> requestStream,
    IServerStreamWriter<ExampleResponse> responseStream, ServerCallContext context)
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                int TempPageIndex = requestStream.Current.PageIndex;
                TempPageIndex++;
                await responseStream.WriteAsync(new ExampleResponse { PageIndex= TempPageIndex, IsDescending=message.IsDescending,PageSize=message.PageSize});
                Console.WriteLine("StreamingBothWays -> Server Received -> PageIndex: " + TempPageIndex);
                if (TempPageIndex > 10)
                    return;
            }
        }
    }
}
