FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Limpiar la caché de NuGet
RUN dotnet nuget locals all --clear

# Copiar primero el archivo de proyecto y restaurar dependencias
COPY ["BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj", "BACKEND/REST_VECINDAPP/"]
RUN dotnet restore "/src/BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj"

# Copiar el resto de los archivos
COPY . .

# Establecer el directorio de trabajo y compilar
WORKDIR "/src/BACKEND/REST_VECINDAPP"
RUN dotnet build "/src/BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/src/BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "REST_VECINDAPP.dll"] 