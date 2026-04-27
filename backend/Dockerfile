# Gebruik de .NET 8 SDK om te bouwen
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopieer de VOLLEDIGE inhoud van de backend map naar de container
# Omdat we de build starten vanuit de 'backend/' map, kopieert dit alles (BL, DAL, UI-MVC, etc.)
COPY . .

# Voer de restore uit op de solution file
RUN dotnet restore "Conversey.sln"

# Bouw en publiceer het UI-MVC project
RUN dotnet publish "UI-MVC/UI-MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Gebruik de kleine runtime image om de app te draaien
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kopieer de gepubliceerde bestanden van de build stage
COPY --from=build /app/publish .

# Maak een lege wwwroot aan om foutmeldingen te voorkomen
RUN mkdir -p wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]