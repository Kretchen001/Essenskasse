FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Modells
WORKDIR /src/Modells
COPY Modells/Modells.csproj .
RUN dotnet restore Modells.csproj

# PWA
WORKDIR /src/PWA
COPY Essenskasse/Essenskasse.csproj .
RUN dotnet restore Essenskasse.csproj

# PWA build
WORKDIR /src
COPY . .
RUN dotnet build ./Essenskasse/Essenskasse.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./Essenskasse/Essenskasse.csproj -c Release -o /app/publish

# nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY Essenskasse/nginx.conf /etc/nginx/nginx.conf