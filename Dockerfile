# Etapa 1: Construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto y restaurar dependencias
COPY ["TiendaVirtual.csproj", "./"]
RUN dotnet restore "TiendaVirtual.csproj"

# Copiar el resto del código
COPY . .
WORKDIR "/src"

# Publicar la aplicación
RUN dotnet publish "TiendaVirtual.csproj" -c Release -o /app/publish

# Etapa 2: Ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar los archivos publicados desde la etapa de construcción
COPY --from=build /app/publish .

# Exponer el puerto 8080 (Railway asigna un puerto, pero la app escuchará en 8080)
EXPOSE 8080

# Establecer variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "TiendaVirtual.dll"]