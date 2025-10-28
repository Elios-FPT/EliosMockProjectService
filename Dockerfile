FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "MockProjectService.sln"
RUN dotnet build "MockProjectService.Web/MockProjectService.Web.csproj" -c Release -o /app/build
RUN dotnet publish "MockProjectService.Web/MockProjectService.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl && apt-get clean

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV Kafka__BootstrapServers=kafka:9092
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s CMD curl --fail http://localhost:80/health || exit 1
ENTRYPOINT ["dotnet", "MockProjectService.Web.dll"]