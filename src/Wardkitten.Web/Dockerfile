# Imagen de la web (Blazor WASM) servida estáticamente por nginx.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Wardkitten.Web/Wardkitten.Web.csproj -c Release -o /app

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/wwwroot /usr/share/nginx/html
COPY src/Wardkitten.Web/nginx.conf /etc/nginx/conf.d/default.conf
# Genera appsettings.Production.json en arranque desde API_BASE_URL (misma imagen para todos los entornos).
COPY src/Wardkitten.Web/40-apibaseurl.sh /docker-entrypoint.d/40-apibaseurl.sh
RUN chmod +x /docker-entrypoint.d/40-apibaseurl.sh
EXPOSE 80
