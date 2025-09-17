# ---- base runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
RUN chown -R app:app /app
USER app
EXPOSE 8080
EXPOSE 8081
# (compose can set ASPNETCORE_URLS; otherwise uncomment)
# ENV ASPNETCORE_URLS=http://+:8080

# ---- build image ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workspace

# Copy solution and ALL referenced csproj files before restore (for proper layer caching)
COPY DMS_SWEN3.sln ./
COPY DMS_SWEN3/DMS_SWEN3.csproj DMS_SWEN3/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY UnitTests/UnitTests.csproj UnitTests/

# Restore at solution level
RUN dotnet restore DMS_SWEN3.sln

# Copy the rest of the source
COPY . .

# Build API project
RUN dotnet build DMS_SWEN3/DMS_SWEN3.csproj -c $BUILD_CONFIGURATION -o /app/build

# ---- publish image ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish DMS_SWEN3/DMS_SWEN3.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- final runtime image ----
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS_SWEN3.dll"]
