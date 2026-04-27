# Gebruik de officiële .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopieer alles vanuit de backend map
COPY . .

# Restore de dependencies
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# Bouw en publiceer
# We voegen extra vlaggen toe om de build niet te laten crashen op missende logo's
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:ErrorOnDuplicatePublishImageFiles=false \
    /p:IgnoreEmptyDirectories=true

# Gebruik de .NET 10 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Zorg dat de map bestaat voor toekomstige uploads/images
RUN mkdir -p wwwroot/images

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]