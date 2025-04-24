FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5193

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ServerHP.Client/ServerHP.Client.csproj ServerHP.Client/

RUN dotnet restore ServerHP.Client/ServerHP.Client.csproj

COPY ServerHP.Client/ ServerHP.Client/

WORKDIR /src/ServerHP.Client
RUN dotnet build ServerHP.Client.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/ServerHP.Client
RUN dotnet publish ServerHP.Client.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerHP.Client.dll"]
