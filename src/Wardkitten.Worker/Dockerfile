# Imagen del worker de evaluación (sin servidor HTTP).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Wardkitten.Worker/Wardkitten.Worker.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=Europe/Madrid
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Wardkitten.Worker.dll"]
