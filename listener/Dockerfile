FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["listener.API/listener.API.csproj", "listener.API/"]
COPY ["listener.Application/listener.Application.csproj", "listener.Application/"]
COPY ["listener.Infrastructure/listener.Infrastructure.csproj", "listener.Infrastructure/"]
COPY ["listener.Domain/listener.Domain.csproj", "listener.Domain/"]
RUN dotnet restore "listener.API/listener.API.csproj"
COPY . .
WORKDIR "/src/listener.API"
RUN dotnet build "listener.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "listener.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY ["listener.API/appsettings.json", "appsettings.json"]
RUN mkdir -p Requests
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "listener.API.dll"]