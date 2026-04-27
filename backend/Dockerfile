# Gebruik de officiële .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopieer de broncode
COPY . .

# HACK: Maak de ontbrekende logo-bestanden aan zodat de SDK niet crasht tijdens publish
RUN mkdir -p UI-MVC/wwwroot/images/ && \
    touch UI-MVC/wwwroot/images/Conversey_logo.png && \
    touch UI-MVC/wwwroot/images/Conversey_logo.svg && \
    touch UI-MVC/wwwroot/images/Conversey_logo_bottom.png && \
    touch UI-MVC/wwwroot/images/Conversey_logo_side.png

# Restore de dependencies
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# Bouw en publiceer
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Gebruik de .NET 10 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Zorg dat de map bestaat voor de runtime
RUN mkdir -p wwwroot/images

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]