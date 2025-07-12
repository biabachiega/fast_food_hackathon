FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/IdentityApi/IdentityApi.csproj ./IdentityApi/
RUN dotnet restore ./IdentityApi/IdentityApi.csproj
COPY src/IdentityApi/. ./IdentityApi/
WORKDIR /src/IdentityApi
RUN dotnet build IdentityApi.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish IdentityApi.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityApi.dll"]