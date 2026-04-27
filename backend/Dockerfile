# --- STAGE 1: Frontend Build ---
FROM node:20-alpine AS client-build
WORKDIR /app

# Kopieer package bestanden vanuit de UI-MVC map
COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

# Kopieer de rest van de frontend broncode
COPY UI-MVC/ ./

# Bouw de frontend (Vite zet de output standaard in de 'dist' map)
RUN pnpm run build

# --- STAGE 2: .NET Build & Publish ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

# Kopieer de solution file en alle .csproj bestanden voor een gecachte restore
# Dit voorkomt dat we bij elke codewijziging alle pakketten opnieuw moeten downloaden
COPY Conversey.sln ./
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/
COPY UI-MVC/UI-MVC.csproj UI-MVC/
COPY Tests/Tests.csproj Tests/

# Voer de restore uit voor de hele solution
RUN dotnet restore

# Kopieer nu de volledige broncode van alle projecten
COPY . .

# Publiceer het UI-MVC project naar de map /app/publish
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- STAGE 3: Final Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kopieer de gepubliceerde .NET bestanden
COPY --from=publish /app/publish .

# Kopieer de frontend build (dist) naar de wwwroot van de .NET app
# We gebruiken hier /app/dist omdat WORKDIR in de eerste stage /app was
COPY --from=client-build /app/dist ./wwwroot

# Poort instellen (standaard voor ASP.NET Core 8 in Docker is 8080)
EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]