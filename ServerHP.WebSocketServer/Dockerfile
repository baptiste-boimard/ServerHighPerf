FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5287

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ServerHP.WebSocketServer/ServerHP.WebSocketServer.csproj ServerHP.WebSocketServer/

RUN dotnet restore ServerHP.WebSocketServer/ServerHP.WebSocketServer.csproj

COPY ServerHP.WebSocketServer/ ServerHP.WebSocketServer/

WORKDIR /src/ServerHP.WebSocketServer
RUN dotnet build ServerHP.WebSocketServer.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/ServerHP.WebSocketServer
RUN dotnet publish ServerHP.WebSocketServer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerHP.WebSocketServer.dll"]
