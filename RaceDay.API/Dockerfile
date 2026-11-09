# Build and run the RaceDay API in Docker Compose.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]
COPY ["RaceDay.Core/RaceDay.Core.csproj", "RaceDay.Core/"]
COPY ["RaceDay.Infrastructure/RaceDay.Infrastructure.csproj", "RaceDay.Infrastructure/"]
RUN dotnet restore "RaceDay.API/RaceDay.API.csproj"

COPY . .
WORKDIR "/src/RaceDay.API"
RUN dotnet publish "RaceDay.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.API.dll"]
