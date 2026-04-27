# Gebruik de officiële .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopieer alles
COPY . .

# Restore
RUN dotnet restore "UI-MVC/UI-MVC.csproj"

# Bouw en publiceer
# De vlag ContinueOnError zorgt dat hij niet stopt bij de missende logo's
RUN dotnet publish "UI-MVC/UI-MVC.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:ContinueOnError=WarnAndContinue

# Gebruik de .NET 10 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p wwwroot/images

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]