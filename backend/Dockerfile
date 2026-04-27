FROM node:20-alpine AS client-build
WORKDIR /src/backend/UI-MVC

COPY UI-MVC/package.json UI-MVC/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY UI-MVC/ ./
RUN pnpm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src/backend

COPY *.sln ./
COPY BL/*.csproj BL/
COPY DAL/*.csproj DAL/
COPY Domain/*.csproj Domain/
COPY UI-MVC/*.csproj UI-MVC/

RUN dotnet restore UI-MVC/UI-MVC.csproj

COPY . .
COPY --from=client-build /src/backend/UI-MVC/wwwroot /src/backend/UI-MVC/wwwroot

RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish /p:SkipClientBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Conversey.UI-MVC.dll"]
