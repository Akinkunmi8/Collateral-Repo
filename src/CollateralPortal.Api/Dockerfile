# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/CollateralPortal.Api/CollateralPortal.Api.csproj", "src/CollateralPortal.Api/"]
COPY ["src/CollateralPortal.Application/CollateralPortal.Application.csproj", "src/CollateralPortal.Application/"]
COPY ["src/CollateralPortal.Infrastructure/CollateralPortal.Infrastructure.csproj", "src/CollateralPortal.Infrastructure/"]
COPY ["src/CollateralPortal.Domain/CollateralPortal.Domain.csproj", "src/CollateralPortal.Domain/"]
RUN dotnet restore "src/CollateralPortal.Api/CollateralPortal.Api.csproj"

COPY . .
WORKDIR /src/src/CollateralPortal.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CollateralPortal.Api.dll"]
