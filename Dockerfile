FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore \
    ./src/bagsisbaku.Api/bagsisbaku.Api.csproj

RUN dotnet publish \
    ./src/bagsisbaku.Api/bagsisbaku.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

RUN dotnet tool install \
    --tool-path /tools \
    dotnet-ef \
    --version 10.0.11

RUN ConnectionStrings__DefaultConnection="Server=127.0.0.1,1433;Database=bagsisbaku_design;User Id=sa;Password=DesignTime_Only_123!;Encrypt=False;TrustServerCertificate=True" \
    Jwt__Issuer="bagsisbaku-build" \
    Jwt__Audience="bagsisbaku-build" \
    Jwt__SigningKey="a2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2traw==" \
    Bootstrap__Identity__Enabled="false" \
    EmailAnnouncements__DemoMode="false" \
    EmailAnnouncements__ProcessingEnabled="false" \
    /tools/dotnet-ef migrations bundle \
    --project ./src/bagsisbaku.Infrastructure/bagsisbaku.Infrastructure.csproj \
    --startup-project ./src/bagsisbaku.Api/bagsisbaku.Api.csproj \
    --context ApplicationDbContext \
    --configuration Release \
    --target-runtime linux-x64 \
    --output /app/migrations/bagsisbaku-migrate

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER root

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=build \
    /app/publish \
    .

COPY --from=build \
    /app/migrations/bagsisbaku-migrate \
    ./bagsisbaku-migrate

RUN chmod +x \
    ./bagsisbaku-migrate

USER $APP_UID

ENTRYPOINT ["dotnet", "bagsisbaku.Api.dll"]