# Meu Projeto Financeiro

Sistema de gerenciamento financeiro inspirado na arquitetura do projeto `ProjetoServicoWork`.

Este repositorio passa a ser o **back-end** da solucao. O front-end React Native fica em uma pasta separada:

- Back-end C#/.NET: `/Users/user/Documents/MeuProjetoFinanceiro`
- Front-end React Native: `/Users/user/Documents/MeuProjetoFinanceiro.Frontend`
- Workspace VS Code: `/Users/user/Documents/MeuProjetoFinanceiro.code-workspace`

## Arquitetura

- `MeuProjetoFinanceiro.Core`: entidades e enums do dominio financeiro.
- `MeuProjetoFinanceiro.Application`: DTOs, contratos e servicos de aplicacao.
- `MeuProjetoFinanceiro.Infrastructure`: EF Core, SQLite, repositorios e inicializacao do banco.
- `MeuProjetoFinanceiro.API`: endpoints REST para contas, categorias, transacoes, orcamentos, dashboard e controle mensal.
- `MeuProjetoFinanceiro.Web`: aplicacao ASP.NET Core MVC com Razor Views, mantida como front legado/local.
- `MeuProjetoFinanceiro.Tests`: projeto base para testes automatizados.

## Como executar

Para rodar apenas a API, que sera consumida pelo React Native:

```bash
dotnet restore
dotnet run --project MeuProjetoFinanceiro.API --urls http://0.0.0.0:8081
```

Com Docker, usando somente o back-end/API:

```bash
docker compose -f docker-compose.backend.yml up -d
```

Com Docker, mantendo API e front Razor legado:

```bash
docker compose up -d
```

Para executar o front-end React Native:

```bash
cd /Users/user/Documents/MeuProjetoFinanceiro.Frontend
npm install
npm start
```

## Front Razor legado

```bash
dotnet restore
dotnet run --project MeuProjetoFinanceiro.Web
```

Para a API:

```bash
dotnet run --project MeuProjetoFinanceiro.API
```

O banco SQLite e criado automaticamente na primeira execucao com dados iniciais para o dashboard.
