FROM node:20-alpine AS client-build
WORKDIR /app

COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY UI-MVC/ ./
RUN pnpm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src
COPY . .

RUN dotnet restore UI-MVC/UI-MVC.csproj
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=client-build /app/dist ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]