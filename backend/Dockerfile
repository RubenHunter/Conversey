# Gebruik de officiële .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopieer alles vanuit de backend map
COPY . .

# Nu de SDK matcht met je global.json, zal de restore vlekkeloos verlopen
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# Bouw en publiceer
RUN dotnet publish "UI-MVC/UI-MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Gebruik de .NET 10 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Maak de wwwroot aan
RUN mkdir -p wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]