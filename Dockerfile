FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj", "BACKEND/REST_VECINDAPP/"]
RUN dotnet restore "BACKEND/REST_VECINDAPP/REST_VECINDAPP.csproj"
COPY . .
WORKDIR "/src/BACKEND/REST_VECINDAPP"
RUN dotnet build "REST_VECINDAPP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "REST_VECINDAPP.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "REST_VECINDAPP.dll"] 