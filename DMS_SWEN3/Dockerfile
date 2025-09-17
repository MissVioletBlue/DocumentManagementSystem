FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
RUN chown -R app:app /app
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workspace
COPY ["DMS_SWEN3.sln", "."]
COPY ["DMS_SWEN3/DMS_SWEN3.csproj", "DMS_SWEN3/"]
RUN dotnet restore "DMS_SWEN3/DMS_SWEN3.csproj"
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "DMS_SWEN3.sln"
COPY . .
RUN dotnet build "DMS_SWEN3/DMS_SWEN3.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DMS_SWEN3/DMS_SWEN3.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS_SWEN3.dll"]
