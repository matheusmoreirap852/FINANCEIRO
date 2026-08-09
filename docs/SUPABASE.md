# Supabase no Back-end C#

O back-end ja suporta SQLite e PostgreSQL. Quando a connection string comeca com `Host=`, `Server=`, `postgres://` ou `postgresql://`, ele usa Npgsql/PostgreSQL.

## Importante

Nao coloque senha do Postgres no React Native. O app mobile deve usar:

- API C# para dados financeiros.
- Supabase publishable key apenas para recursos seguros no cliente, como Auth.

A senha do banco deve ficar somente no back-end, em `.env`, Docker, ou variavel de ambiente local.

## Connection string pelo pooler

Pelo print, voce esta usando o pooler do Supabase:

```text
Host=aws-1-us-west-2.pooler.supabase.com
Port=6543
Database=postgres
Username=postgres.PROJECT_REF
Password=SUA_SENHA
```

No `.env` do back-end, use:

```bash
ASPNETCORE_ENVIRONMENT=Development
Database__Schema=controle_financeiro
ConnectionStrings__DefaultConnection=Host=aws-1-us-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.PROJECT_REF;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true
```

Troque:

- `PROJECT_REF` pelo sufixo do usuario exibido no Supabase.
- `SUA_SENHA` pela senha do banco.
- `controle_financeiro` pelo nome exato do schema que voce criou.

## Rodar local com Docker

O Docker Compose le o `.env` automaticamente:

```bash
docker compose -f docker-compose.backend.yml up -d
```

## Rodar local sem Docker

Use o script:

```bash
./scripts/run-api-local.sh
```

Ele carrega as variaveis do `.env` e roda a API na porta `8081`.

## Testar

```bash
curl http://localhost:8081/api/dashboard
curl http://localhost:8081/api/ControleMensal/2026
```

## Observacao sobre schema

Se o banco Supabase estiver vazio, o inicializador do projeto cria as tabelas usadas pela aplicacao. Para controle mais formal no futuro, podemos trocar isso por migrations do EF Core.

## Migrar dados do SQLite local para o schema do Supabase

Para gerar um SQL com as mesmas tabelas e os dados do `financeiro.db`:

```bash
./scripts/generate-supabase-migration.sh controle_financeiro
```

Troque `controle_financeiro` pelo nome exato do schema criado no Supabase.

O arquivo sera criado em:

```text
artifacts/supabase-migration-controle_financeiro.sql
```

Depois abra esse arquivo no DBeaver ou no SQL Editor do Supabase e execute conectado no banco `postgres`.

Depois de migrar, configure a API para usar o mesmo schema:

```bash
Database__Schema=controle_financeiro
ConnectionStrings__DefaultConnection=Host=aws-1-us-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.PROJECT_REF;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true
```
