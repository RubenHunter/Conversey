FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopieer ALLES van de backend map naar de container
COPY . .

# We negeren de .sln file even en gaan direct naar het hoofdproject.
# De --no-cache dwingt hem om vers te kijken.
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# We publiceren het project. Dotnet zoekt zelf de afhankelijke projecten (BL, DAL) 
# in de mappen die we net met 'COPY . .' hebben binnengehaald.
RUN dotnet publish "UI-MVC/UI-MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Zorg dat de map voor statische bestanden bestaat
RUN mkdir -p wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]