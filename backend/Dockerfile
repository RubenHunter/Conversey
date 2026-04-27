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

# 3. DE SUPER-HACK: Maak de bestanden fysiek aan op elk mogelijk pad
# We doen dit VOOR de publish.
RUN mkdir -p UI-MVC/wwwroot/images/ && \
    echo "dummy" > UI-MVC/wwwroot/images/Conversey_logo.png && \
    echo "dummy" > UI-MVC/wwwroot/images/Conversey_logo.svg && \
    echo "dummy" > UI-MVC/wwwroot/images/Conversey_logo_bottom.png && \
    echo "dummy" > UI-MVC/wwwroot/images/Conversey_logo_side.png && \
    mkdir -p UI-MVC/wwwroot/.vite/ && \
    echo "{}" > UI-MVC/wwwroot/.vite/manifest.json

# 4. Restore
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# 5. Publish met ALLES uitgeschakeld wat met compressie te maken heeft
# We voegen /p:ExcludeStaticWebAssets=true toe als laatste redmiddel
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishCompressedAssets=false \
    /p:DisableStaticWebAssetsFingerprinting=true \
    /p:ResolveStaticWebAssetsInputs=true 

# ==========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Omdat we de assets negeerden in de build, moeten we zorgen dat de 
# wwwroot map van de build stage ook echt mee overgaat als die nog niet in /app/publish zat.
# (Vite zet ze namelijk in UI-MVC/wwwroot)
COPY --from=build /src/UI-MVC/wwwroot ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI_MVC.dll"]