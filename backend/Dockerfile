# 1. Frontend Build
FROM node:20-alpine AS client-build
WORKDIR /src
RUN corepack enable && corepack prepare pnpm@latest --activate

# Kopieer alleen de frontend bestanden
COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./UI-MVC/
WORKDIR /src/UI-MVC
RUN pnpm install

COPY UI-MVC/ ./
RUN pnpm run build

# 2. .NET Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopieer projectbestanden
COPY *.sln ./
COPY BL/*.csproj BL/
COPY DAL/*.csproj DAL/
COPY Domain/*.csproj Domain/
COPY UI-MVC/*.csproj UI-MVC/
RUN dotnet restore UI-MVC/UI-MVC.csproj

# Kopieer de rest van de broncode
COPY . .

# Kopieer de GEBOUWDE frontend van de vorige stage
COPY --from=client-build /src/UI-MVC/wwwroot ./wwwroot

# Publiceer de applicatie
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:SkipClientBuild=true

# 3. Final Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
RUN apk add --no-cache curl krb5-libs icu-libs libintl
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=40s \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Conversey.UI_MVC.dll"]
