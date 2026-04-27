# ==========================================
# BUILD STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Installeer Node.js (Vite heeft dit nodig)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# 2. Installeer pnpm global (jouw csproj gebruikt pnpm)
RUN npm install -g pnpm

# 3. Kopieer de volledige solution
COPY . .

# 4. HACK: Maak de mappen en bestanden aan waar de .csproj naar zoekt.
# Dit voorkomt de MSB3030 en StaticWebAsset errors.
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
# We voegen extra vlaggen toe om de asset-optimalisatie die crasht te omzeilen.
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishCompressedAssets=false \
    /p:DisableStaticWebAssetsFingerprinting=true

# ==========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Kopieer de gepubliceerde bestanden van de build stage
COPY --from=build /app/publish .

# Zorg dat de poort openstaat
EXPOSE 8080

# Start de applicatie
ENTRYPOINT ["dotnet", "Conversey.UI_MVC.dll"]