FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VehicleService/VehicleService.csproj", "VehicleService/"]
COPY ["KargoTakip.Infrastructure/KargoTakip.Infrastructure.csproj", "KargoTakip.Infrastructure/"]
RUN dotnet restore "VehicleService/VehicleService.csproj"
COPY . .
WORKDIR "/src/VehicleService"
RUN dotnet build "VehicleService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VehicleService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VehicleService.dll"]