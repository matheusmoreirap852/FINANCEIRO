FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MeuProjetoFinanceiro.slnx ./
COPY MeuProjetoFinanceiro.Core/MeuProjetoFinanceiro.Core.csproj MeuProjetoFinanceiro.Core/
COPY MeuProjetoFinanceiro.Application/MeuProjetoFinanceiro.Application.csproj MeuProjetoFinanceiro.Application/
COPY MeuProjetoFinanceiro.Infrastructure/MeuProjetoFinanceiro.Infrastructure.csproj MeuProjetoFinanceiro.Infrastructure/
COPY MeuProjetoFinanceiro.API/MeuProjetoFinanceiro.API.csproj MeuProjetoFinanceiro.API/

RUN dotnet restore MeuProjetoFinanceiro.API/MeuProjetoFinanceiro.API.csproj

COPY . .
RUN dotnet publish MeuProjetoFinanceiro.API/MeuProjetoFinanceiro.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8081
ENV ASPNETCORE_ENVIRONMENT=Production

RUN mkdir -p /data
COPY --from=build /app/publish .

EXPOSE 8081
ENTRYPOINT ["dotnet", "MeuProjetoFinanceiro.API.dll"]
