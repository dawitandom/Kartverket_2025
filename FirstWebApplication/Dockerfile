# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopier csproj fra riktig path (uten klammer)
COPY FirstWebApplication/FirstWebApplication.csproj FirstWebApplication/
RUN dotnet restore "FirstWebApplication/FirstWebApplication.csproj"

# Kopier resten av prosjektet
COPY . .
WORKDIR "/src/FirstWebApplication"
RUN dotnet build "FirstWebApplication.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "FirstWebApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN mkdir -p /keys && chmod -R 777 /keys
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FirstWebApplication.dll"]
