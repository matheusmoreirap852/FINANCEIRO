# Meu Projeto Financeiro

Sistema de gerenciamento financeiro inspirado na arquitetura do projeto `ProjetoServicoWork`.

## Arquitetura

- `MeuProjetoFinanceiro.Core`: entidades e enums do dominio financeiro.
- `MeuProjetoFinanceiro.Application`: DTOs, contratos e servicos de aplicacao.
- `MeuProjetoFinanceiro.Infrastructure`: EF Core, SQLite, repositorios e inicializacao do banco.
- `MeuProjetoFinanceiro.API`: endpoints REST para contas, categorias, transacoes, orcamentos e dashboard.
- `MeuProjetoFinanceiro.Web`: aplicacao ASP.NET Core MVC com Razor Views.
- `MeuProjetoFinanceiro.Tests`: projeto base para testes automatizados.

## Como executar

```bash
dotnet restore
dotnet run --project MeuProjetoFinanceiro.Web
```

Para a API:

```bash
dotnet run --project MeuProjetoFinanceiro.API
```

O banco SQLite e criado automaticamente na primeira execucao com dados iniciais para o dashboard.
