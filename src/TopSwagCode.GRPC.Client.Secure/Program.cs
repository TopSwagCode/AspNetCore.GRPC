using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TopSwagCode.GRPC.Server.Secure;
using static TopSwagCode.GRPC.Server.Secure.Greeter;
using static TopSwagCode.GRPC.Server.Secure.WeatherForecasts;

namespace TopSwagCode.GRPC.Client.Secure
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            // Show insecure call that fails.
            await GreeterRequestInsecure(channel);

            var token = await LoginRequest();

            await GreeterRequest(channel, token);
            await WeatherForecastsRequest(channel, token);

            using var secureChannel = CreateAuthenticatedChannel("https://localhost:5001", token);

            await GreeterRequestWithSecureChannel(secureChannel);
            await WeatherForecastsRequestWithSecureChannel(secureChannel);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task<string> LoginRequest()
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://localhost:5001/generateJwtToken?name=TopSwagCode"),
                Method = HttpMethod.Get,
                Version = new Version(2, 0)
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task GreeterRequestInsecure(GrpcChannel channel)
        {
            try
            {
                var client = new GreeterClient(channel);
                var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });

                Console.WriteLine("Greeting: " + reply.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch(Grpc.Core.RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated)
            {
                Console.WriteLine("Failed to make insecure Call to secure endpoint. Thats good! :)");
            }
            
        }


        private static async Task GreeterRequest(GrpcChannel channel, string token)
        {

            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");

            var client = new GreeterClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" }, headers);

            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task WeatherForecastsRequest(GrpcChannel channel, string token)
        {
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");

            var client = new WeatherForecastsClient(channel);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var streamingCall = client.GetWeatherStream(new Empty(), cancellationToken: cts.Token, headers: headers);

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


        private static async Task GreeterRequestWithSecureChannel(GrpcChannel channel)
        {
            var client = new GreeterClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });

            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task WeatherForecastsRequestWithSecureChannel(GrpcChannel channel)
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


        private static GrpcChannel CreateAuthenticatedChannel(string address, string token)
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {token}");
                return Task.CompletedTask;
            });

            // SslCredentials is used here because this channel is using TLS.
            // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            return channel;
        }
    }
}
