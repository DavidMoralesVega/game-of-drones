FROM node:24-alpine AS web
WORKDIR /web
COPY src/GameOfDrones.Web/package*.json ./
RUN npm ci
COPY src/GameOfDrones.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY Directory.Build.props ./
COPY src/GameOfDrones.Domain/GameOfDrones.Domain.csproj src/GameOfDrones.Domain/
COPY src/GameOfDrones.Infrastructure/GameOfDrones.Infrastructure.csproj src/GameOfDrones.Infrastructure/
COPY src/GameOfDrones.Api/GameOfDrones.Api.csproj src/GameOfDrones.Api/
RUN dotnet restore src/GameOfDrones.Api/GameOfDrones.Api.csproj
COPY src/GameOfDrones.Domain/ src/GameOfDrones.Domain/
COPY src/GameOfDrones.Infrastructure/ src/GameOfDrones.Infrastructure/
COPY src/GameOfDrones.Api/ src/GameOfDrones.Api/
RUN dotnet publish src/GameOfDrones.Api/GameOfDrones.Api.csproj -c Release -o /app -p:SkipSpaBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=api /app ./
COPY --from=web /web/dist ./wwwroot
RUN mkdir -p /app/data && chown app:app /app/data
ENV ConnectionStrings__GameOfDrones="Data Source=/app/data/gameofdrones.db"
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "GameOfDrones.Api.dll"]
