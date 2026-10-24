FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Modells
WORKDIR /src/Modells
COPY Modells/Modells.csproj .
RUN dotnet restore Modells.csproj

# API
WORKDIR /src/API
COPY EssenskasseAPI/EssenskasseAPI.csproj .
RUN dotnet restore EssenskasseAPI.csproj

# API build
WORKDIR /src
COPY . .
RUN dotnet build ./EssenskasseAPI/EssenskasseAPI.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./EssenskasseAPI/EssenskasseAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# Laufzeit-Image statt SDK: deutlich kleiner und ohne Build-Werkzeuge
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EssenskasseAPI.dll"]
