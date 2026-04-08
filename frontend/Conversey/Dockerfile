# Stage 1: Build de applicatie
FROM node:20-slim AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Serveer met Nginx
FROM nginx:stable-alpine

# FIX: Zorg dat Nginx alle routes naar index.html stuurt (voorkomt 404's bij refreshen)
RUN sed -i 's/index  index.html index.htm;/index  index.html index.htm; try_files $uri $uri\/ \/index.html;/g' /etc/nginx/conf.d/default.conf

# Kopieer de build output naar de Nginx map
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]