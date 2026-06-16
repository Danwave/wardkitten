#!/bin/sh
# Inyecta la URL base de la API en la app WASM en tiempo de arranque del contenedor.
set -e
: "${API_BASE_URL:=https://api.wardkitten.com}"
printf '{ "ApiBaseUrl": "%s" }\n' "$API_BASE_URL" > /usr/share/nginx/html/appsettings.Production.json
echo "[wardkitten-web] ApiBaseUrl = $API_BASE_URL"
