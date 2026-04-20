# Conversey

Conversey now uses an integrated ASP.NET + Vite structure:

- Backend API and host app: `backend/REST`
- Frontend Vite app: `backend/REST/ClientApp`
- Production frontend assets: `backend/REST/wwwroot`

## Project Structure

- `backend/BL`, `backend/DAL`, `backend/Domain`, `backend/REST`, `backend/Tests`
- `backend/Conversey.sln` includes a `ClientApp` solution folder with key frontend files.

## Local Development

Run backend API host:

```bash
dotnet run --project /home/ruben/Projects/development/backend/REST/REST.csproj
```

Run Vite frontend (HMR):

```bash
cd /home/ruben/Projects/development/backend/REST/ClientApp
corepack enable
pnpm install
pnpm run dev
```

Development behavior:

- API routes are served from `http://localhost:5231/api/...`
- If `wwwroot/index.html` does not exist, non-API routes on backend redirect to Vite (`http://localhost:5173`)
- If `wwwroot/index.html` exists, backend can serve SPA directly

## Build Frontend

```bash
cd /home/ruben/Projects/development/backend/REST/ClientApp
pnpm run build
```

This writes static files to `backend/REST/wwwroot`.

## Publish Backend

```bash
dotnet publish /home/ruben/Projects/development/backend/REST/REST.csproj -c Release
```

`REST.csproj` runs the ClientApp build during publish (unless `SkipClientBuild=true` is set).

## Docker

Compose now runs integrated backend + postgres:

```bash
cd /home/ruben/Projects/development
docker compose up --build
```

- Backend: `http://localhost:5231`
- Postgres: `localhost:5432`

