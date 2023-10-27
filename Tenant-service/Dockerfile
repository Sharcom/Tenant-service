#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5002
EXPOSE 6002

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Tenant-service/Tenant-service.csproj", "Tenant-service/"]
RUN dotnet restore "Tenant-service/Tenant-service.csproj"
COPY . .
WORKDIR "/src/Tenant-service"
RUN dotnet build "Tenant-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tenant-service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tenant-service.dll"]