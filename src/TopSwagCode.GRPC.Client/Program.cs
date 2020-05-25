using Grpc.Net.Client;
using System;
using System.Threading.Tasks;
using TopSwagCode.GRPC.Server;
using static TopSwagCode.GRPC.Server.Greeter;

namespace TopSwagCode.GRPC.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            await GreeterRequest(channel);
        }

        private static async Task GreeterRequest(GrpcChannel channel)
        {
            var client = new GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                                new HelloRequest { Name = "GreeterClient" });
            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey();
        }
    }
}
