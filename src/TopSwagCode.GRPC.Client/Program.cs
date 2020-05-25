using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TopSwagCode.GRPC.Server;
using static TopSwagCode.GRPC.Server.Greeter;
using static TopSwagCode.GRPC.Server.WeatherForecasts;

namespace TopSwagCode.GRPC.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            await GreeterRequest(channel);
            await WeatherForecastsRequest(channel);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task GreeterRequest(GrpcChannel channel)
        {
            var client = new GreeterClient(channel);
            var reply = await client.SayHelloAsync( new HelloRequest { Name = "GreeterClient" });

            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task WeatherForecastsRequest(GrpcChannel channel)
        {
            var client = new WeatherForecastsClient(channel);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var streamingCall = client.GetWeatherStream(new Empty(), cancellationToken: cts.Token);

            try
            {
                await foreach (var weatherData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    Console.WriteLine($"{weatherData.DateTimeStamp.ToDateTime():s} | {weatherData.Summary} | {weatherData.TemperatureC} C");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream cancelled.");
            }
            catch (IOException) // https://github.com/dotnet/runtime/issues/1586
            {
                Console.WriteLine("Client and server disagree on active stream count.");
            }
        }
    }
}
