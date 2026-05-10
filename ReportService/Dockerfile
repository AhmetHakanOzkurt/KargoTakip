FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ReportService/ReportService.csproj", "ReportService/"]
COPY ["KargoTakip.Infrastructure/KargoTakip.Infrastructure.csproj", "KargoTakip.Infrastructure/"]
RUN dotnet restore "ReportService/ReportService.csproj"
COPY . .
WORKDIR "/src/ReportService"
RUN dotnet build "ReportService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportService.dll"]