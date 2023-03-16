using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcGreeter;

namespace GrpcGreeterClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            //Basic request
            //var reply = await client.SayHelloAsync(
            //                  new HelloRequest { Name = "GreeterClient" });

            //Unary call
            var reply = await client.UnaryCallAsync(
                new ExampleRequest { PageSize = 33, PageIndex = 2, IsDescending = false }
                );
            Console.WriteLine("UnaryCallResponse -> PageSize: " + reply.PageSize);

            Console.WriteLine();

            //StreamingFromServer
            var call = client.StreamingFromServer(new ExampleRequest { PageSize = 33, PageIndex = 2, IsDescending = false });
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("StreamingFromServer -> PageIndex: : " + call.ResponseStream.Current.PageIndex);
            }

            Console.WriteLine();

            //StreamingFromClient
            using var StreamingFromClientCall = client.StreamingFromClient();

            int TempPageIndex = 2;
            for(int i=0;i<5;i++) { 
            await StreamingFromClientCall.RequestStream.WriteAsync(new ExampleRequest { PageSize = 33, PageIndex = TempPageIndex, IsDescending = false });
            TempPageIndex++;
            }
            Console.WriteLine("Check the server console ...");

            Console.WriteLine();

            //Bi-directional streaming call
            using var BiDirectionalCall = client.StreamingBothWays();

            await BiDirectionalCall.RequestStream.WriteAsync(new ExampleRequest { PageSize = 33, PageIndex = 2, IsDescending = false });
            while (await BiDirectionalCall.ResponseStream.MoveNext())
            {
                await BiDirectionalCall.RequestStream.WriteAsync(new ExampleRequest { PageIndex = BiDirectionalCall.ResponseStream.Current.PageIndex, IsDescending = BiDirectionalCall.ResponseStream.Current.IsDescending, PageSize = BiDirectionalCall.ResponseStream.Current.PageSize });
                Console.WriteLine("StreamingBothWays -> Client Received -> PageIndex: " + BiDirectionalCall.ResponseStream.Current.PageIndex);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

}
