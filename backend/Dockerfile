# --- STAGE 1: Frontend Build ---
FROM node:20-alpine AS client-build
WORKDIR /app

# Kopieer package bestanden
COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

# Kopieer de rest van de UI-MVC map
COPY UI-MVC/ ./

# Bouw de frontend. Dit maakt de map '/app/dist' aan omdat WORKDIR /app is.
# Mocht Vite het toch in een submap zetten, vangen we dat op in de laatste stap.
RUN pnpm run build

# --- STAGE 2: .NET Build & Publish ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

COPY Conversey.sln ./
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/
COPY UI-MVC/UI-MVC.csproj UI-MVC/
COPY Tests/Tests.csproj Tests/

RUN dotnet restore
COPY . .
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- STAGE 3: Final Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# DE FIX: We kopiëren de inhoud van de dist map. 
# Omdat we in Stage 1 'WORKDIR /app' gebruikten en de bestanden van UI-MVC 
# direct in /app hebben gezet, staat de output in /app/dist.
COPY --from=client-build /app/dist ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]