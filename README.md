# GRPC.DotnetCore

gRPC stand for 

## Overview

> In gRPC, a client application can directly call a method on a server application on a different machine as if it were a local object, making it easier for you to create distributed applications and services. As in many RPC systems, gRPC is based around the idea of defining a service, specifying the methods that can be called remotely with their parameters and return types. On the server side, the server implements this interface and runs a gRPC server to handle client calls. On the client side, the client has a stub (referred to as just a client in some languages) that provides the same methods as the server.
> ![grpc](assets/grpc-example.svg)
> gRPC clients and servers can run and talk to each other in a variety of environments - from servers inside Google to your own desktop - and can be written in any of gRPC’s supported languages. So, for example, you can easily create a gRPC server in Java with clients in Go, Python, or Ruby. In addition, the latest Google APIs will have gRPC versions of their interfaces, letting you easily build Google functionality into your applications.

Taken from [gRPC.io](https://grpc.io/docs/guides/)

## Working with gRPC in Dotnet Core.

### Server

We can start by using the default gRPC template that dotnet provides us.

![template](assets/template.png)

Creating a new project using the template we can find a .proto file that looks like this describing the message format.

```proto
    syntax = "proto3";

    option csharp_namespace = "TopSwagCode.GRPC.Server";

    package greet;

    // The greeting service definition.
    service Greeter {
    // Sends a greeting
    rpc SayHello (HelloRequest) returns (HelloReply);
    }

    // The request message containing the user's name.
    message HelloRequest {
    string name = 1;
    }

    // The response message containing the greetings.
    message HelloReply {
    string message = 1;
    }

```

If we look inside the csproj file, we can see the proto file is included as a protobuf gRPC Service running on a server. This will generate code for us to easy implementing a service.

```xml
    <ItemGroup>
    <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
    </ItemGroup>
```

We can take a look at the code generated. Small part can be seen in screenshot below.

![generated](assets/generated.png)

Now we can check out the implementation of the service logic that uses the generated code.

```csharp
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
    }
```

Last thing to check out is how routing is handled. You can find this in Startup.cs and it looks similar to how normal API are setup.

```csharp
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
```

### Client

Well doesn't make much sense to create a gRPC server without also showing how to interact with it. We can start by creating a simple Console app.

![console](assets/console.png)