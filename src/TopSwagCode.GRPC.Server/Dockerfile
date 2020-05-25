#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TopSwagCode.GRPC.Server/TopSwagCode.GRPC.Server.csproj", "TopSwagCode.GRPC.Server/"]
RUN dotnet restore "TopSwagCode.GRPC.Server/TopSwagCode.GRPC.Server.csproj"
COPY . .
WORKDIR "/src/TopSwagCode.GRPC.Server"
RUN dotnet build "TopSwagCode.GRPC.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TopSwagCode.GRPC.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TopSwagCode.GRPC.Server.dll"]