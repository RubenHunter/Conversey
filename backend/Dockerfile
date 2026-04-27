# ==========================================
# BUILD STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Installeer Node.js & pnpm
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g pnpm

# 2. Kopieer alles
COPY . .

# 3. Maak de bestanden aan EN zorg dat ze niet leeg zijn (sommige tools skippen 0-byte files)
RUN mkdir -p UI-MVC/wwwroot/images/ && \
    echo "svg-data" > UI-MVC/wwwroot/images/Conversey_logo.svg && \
    echo "png-data" > UI-MVC/wwwroot/images/Conversey_logo.png && \
    echo "png-data" > UI-MVC/wwwroot/images/Conversey_logo_bottom.png && \
    echo "png-data" > UI-MVC/wwwroot/images/Conversey_logo_side.png && \
    mkdir -p UI-MVC/wwwroot/.vite/ && \
    echo '{"main.js":"main.js"}' > UI-MVC/wwwroot/.vite/manifest.json

# 4. Restore
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# 5. De "Nuke" Publish
# We zetten de compressie op 'None' via de interne vlag van de Web SDK
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishCompressedAssets=false \
    /p:StaticWebAssetCompressionLevel=None \
    /p:DisableStaticWebAssetsFingerprinting=true

# ==========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI_MVC.dll"]