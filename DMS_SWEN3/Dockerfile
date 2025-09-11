FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DMS_SWEN3/DMS_SWEN3.csproj", "DMS_SWEN3/"]
RUN dotnet restore "DMS_SWEN3/DMS_SWEN3.csproj"
COPY . .
WORKDIR "/src/DMS_SWEN3"
RUN dotnet build "DMS_SWEN3.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DMS_SWEN3.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS_SWEN3.dll"]
