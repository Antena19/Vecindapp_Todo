FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Limpiar la caché de NuGet
RUN dotnet nuget locals all --clear

# Crear directorios necesarios
RUN mkdir -p /src/BACKEND/REST_VECINDAPP

# Copiar primero el archivo de proyecto y restaurar dependencias
COPY ["BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj", "/src/BACKEND/REST_VECINDAPP/"]
RUN dotnet restore "/src/BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj"

# Copiar el resto de los archivos
COPY . /src/

# Compilar
RUN dotnet build "/src/BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/src/BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "REST_VECINDAPP.dll"] 