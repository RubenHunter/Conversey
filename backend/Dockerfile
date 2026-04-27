# 1. Build stage voor de Frontend
FROM node:20-alpine AS client-build
WORKDIR /app
# Gebruik UI-MVC map voor frontend assets
COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile
COPY UI-MVC/ ./
RUN pnpm run build

# 2. Build stage voor .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

# Kopieer de solution en ALLE projectbestanden eerst voor een efficiënte restore
COPY Conversey.sln ./
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/
COPY UI-MVC/UI-MVC.csproj UI-MVC/
COPY Tests/Tests.csproj Tests/

# Restore alle dependencies
RUN dotnet restore

# Kopieer nu de rest van de broncode
COPY . .

# Publiceer het UI-MVC project
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:UseAppHost=false

# 3. Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Kopieer de frontend build naar de wwwroot
COPY --from=client-build /app/dist ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]