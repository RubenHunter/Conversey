FROM node:20-alpine AS client-build
WORKDIR /src/backend/REST/ClientApp

COPY REST/ClientApp/package.json REST/ClientApp/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY REST/ClientApp/ ./
RUN pnpm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src/backend

COPY *.sln ./
COPY BL/*.csproj BL/
COPY DAL/*.csproj DAL/
COPY Domain/*.csproj Domain/
COPY REST/*.csproj REST/

RUN dotnet restore REST/REST.csproj

COPY . .
COPY --from=client-build /src/backend/REST/wwwroot /src/backend/REST/wwwroot

RUN dotnet publish REST/REST.csproj -c Release -o /app/publish /p:SkipClientBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Conversey.REST.dll"]
