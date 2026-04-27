# Gebruik de officiële .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Installeer Node.js (benodigd voor Vite/pnpm)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# 2. Installeer pnpm (omdat je csproj pnpm gebruikt)
RUN npm install -g pnpm

# Kopieer de broncode
COPY . .

# HACK: Mappenstructuur sussen (mocht je de csproj nog niet hebben aangepast)
RUN mkdir -p UI-MVC/wwwroot/images/ && \
    touch UI-MVC/wwwroot/images/Conversey_logo.png && \
    touch UI-MVC/wwwroot/images/Conversey_logo.svg && \
    touch UI-MVC/wwwroot/images/Conversey_logo_bottom.png && \
    touch UI-MVC/wwwroot/images/Conversey_logo_side.png

# Restore de .NET dependencies
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# 3. Bouw en publiceer (dit triggert nu succesvol de pnpm build in je csproj)
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Gebruik de .NET 10 ASP.NET runtime voor de uiteindelijke image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI_MVC.dll"]