# --- STAGE 1: Frontend Build ---
FROM node:20-alpine AS client-build
# We zetten de WORKDIR op de UI-MVC map zelf
WORKDIR /app/UI-MVC

# Kopieer package bestanden (relatief aan de backend map)
COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

# Kopieer de rest van de UI-MVC broncode
COPY UI-MVC/ ./

# Bouw de frontend -> dit maakt de map /app/UI-MVC/dist aan
RUN pnpm run build

# --- STAGE 2: .NET Build & Publish ---
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

# --- STAGE 3: Final Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# DE FIX: We kopiëren nu specifiek vanuit de submap waar Vite de bestanden dumpt
COPY --from=client-build /app/UI-MVC/dist ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]