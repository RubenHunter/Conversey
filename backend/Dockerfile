# --- STAGE 1: .NET Build & Publish ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

# Kopieer alles voor de .NET restore
COPY Conversey.sln ./
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/
COPY UI-MVC/UI-MVC.csproj UI-MVC/
COPY Tests/Tests.csproj Tests/

RUN dotnet restore
COPY . .
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- STAGE 2: Final Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# We maken een lege wwwroot aan zodat de app niet crasht
RUN mkdir -p wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]