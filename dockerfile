# Etapa 1: Construcción (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar los archivos de proyecto (.csproj) de todas las capas para restaurar dependencias
# Esto optimiza la caché de Docker si no has cambiado dependencias
COPY ["Lab10_RodrigoApaza.Api/Lab10_RodrigoApaza.Api.csproj", "Lab10_RodrigoApaza.Api/"]
COPY ["Lab10_RodrigoApaza.Application/Lab10_RodrigoApaza.Application.csproj", "Lab10_RodrigoApaza.Application/"]
COPY ["Lab10_RodrigoApaza.Domain/Lab10_RodrigoApaza.Domain.csproj", "Lab10_RodrigoApaza.Domain/"]
COPY ["Lab10_RodrigoApaza.Infrastructure/Lab10_RodrigoApaza.Infrastructure.csproj", "Lab10_RodrigoApaza.Infrastructure/"]

# Restaurar dependencias (basado en el proyecto principal que referencia a los demás)
RUN dotnet restore "Lab10_RodrigoApaza.Api/Lab10_RodrigoApaza.Api.csproj"

# Copiar el resto del código fuente
COPY . .

# Compilar la aplicación
WORKDIR "/src/Lab10_RodrigoApaza.Api"
RUN dotnet build "Lab10_RodrigoApaza.Api.csproj" -c Release -o /app/build

# Publicar la aplicación (genera los archivos finales .dll)
FROM build AS publish
RUN dotnet publish "Lab10_RodrigoApaza.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime (Imagen final ligera para ejecutar)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiar los archivos publicados desde la etapa de construcción
COPY --from=publish /app/publish .

# Exponer el puerto 8080 (puerto por defecto en contenedores .NET 8/9)
EXPOSE 8080

# Definir el punto de entrada
ENTRYPOINT ["dotnet", "Lab10_RodrigoApaza.Api.dll"]