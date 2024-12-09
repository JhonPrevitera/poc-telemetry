FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["POC_SpanOpenTelemetry.Api/POC_SpanOpenTelemetry.Api.csproj", "POC_SpanOpenTelemetry.Api/"]
RUN dotnet restore "POC_SpanOpenTelemetry.Api/POC_SpanOpenTelemetry.Api.csproj"
COPY . .
WORKDIR "/src/POC_SpanOpenTelemetry.Api"
RUN dotnet build "POC_SpanOpenTelemetry.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "POC_SpanOpenTelemetry.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POC_SpanOpenTelemetry.Api.dll"]
