# Etapa 1: Construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar SOLO el archivo del proyecto primero
COPY ["TiendaVirtual.csproj", "./"]
RUN dotnet restore "TiendaVirtual.csproj"

# Copiar el resto del código
COPY . .
WORKDIR "/src"

# Publicar la aplicación
RUN dotnet publish "TiendaVirtual.csproj" -c Release -o /app/publish

# Etapa 2: Ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar desde la etapa de build
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

ENTRYPOINT ["dotnet", "TiendaVirtual.dll"]