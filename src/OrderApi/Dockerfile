FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/OrderApi/OrderApi.csproj ./OrderApi/
RUN dotnet restore ./OrderApi/OrderApi.csproj
COPY src/OrderApi/. ./OrderApi/
WORKDIR /src/OrderApi
RUN dotnet build OrderApi.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish OrderApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderApi.dll"]