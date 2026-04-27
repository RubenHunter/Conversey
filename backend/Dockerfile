# ==========================================
# BUILD STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Installeer Node.js (Vite heeft dit nodig)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# 2. Installeer pnpm global
RUN npm install -g pnpm

# 3. Kopieer de broncode
COPY . .

# 4. HACK: Maak dummy bestanden aan zodat de compiler niet klaagt over ontbrekende bronnen
RUN mkdir -p UI-MVC/wwwroot/images/ && \
    touch UI-MVC/wwwroot/images/Conversey_logo.png && \
    touch UI-MVC/wwwroot/images/Conversey_logo.svg && \
    touch UI-MVC/wwwroot/images/Conversey_logo_bottom.png && \
    touch UI-MVC/wwwroot/images/Conversey_logo_side.png && \
    mkdir -p UI-MVC/wwwroot/.vite/ && \
    touch UI-MVC/wwwroot/.vite/manifest.json

# 5. Restore .NET packages
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# 6. Publish de applicatie
# We voegen /p:ResolveStaticWebAssetsInputs=false toe om de corrupte asset-pipeline te negeren.
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishCompressedAssets=false \
    /p:DisableStaticWebAssetsFingerprinting=true \
    /p:ResolveStaticWebAssetsInputs=false

# ==========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Kopieer de output van de build stage
COPY --from=build /app/publish .

# Exposeer de poort
EXPOSE 8080

# Start de applicatie
ENTRYPOINT ["dotnet", "Conversey.UI_MVC.dll"]