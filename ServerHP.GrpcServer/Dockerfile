FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5024

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ServerHP.GrpcServer/ServerHP.GrpcServer.csproj ServerHP.GrpcServer/
RUN dotnet restore ServerHP.GrpcServer/ServerHP.GrpcServer.csproj

COPY ServerHP.GrpcServer/ ServerHP.GrpcServer/

WORKDIR /src/ServerHP.GrpcServer
RUN dotnet build ServerHP.GrpcServer.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/ServerHP.GrpcServer
RUN dotnet publish ServerHP.GrpcServer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerHP.GrpcServer.dll"]
