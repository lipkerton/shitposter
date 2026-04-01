FROM mcr.microsoft.com/dotnet/runtime:10.0@sha256:57bbbdc0a616a7981ba65d7e16acff89769bdd2b88b7df3039ed6d5d281fff59 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:641ab8d0a499b924d685c6c5af45e6ecb2b42af6290fa399c8f858d3c1823829 AS build
WORKDIR /src
COPY ["listener.csproj", "."]
RUN dotnet restore "listener.csproj"
COPY . .
RUN dotnet build "listener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "listener.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "listener.dll"]