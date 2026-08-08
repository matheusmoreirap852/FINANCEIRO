# CI/CD com GitHub Actions e EC2

Este projeto usa dois repositorios:

- Back-end: `matheusmoreirap852/FINANCEIRO`
- Front-end: `matheusmoreirap852/front-endProjetoFinanceiro`

O deploy principal fica no repositorio do back-end. O GitHub Actions builda as imagens Docker, publica no GHCR e a EC2 apenas baixa as imagens e reinicia os containers.

## Fluxo recomendado

1. Criar uma branch para a mudanca.
2. Abrir Pull Request.
3. Esperar o CI passar.
4. Fazer merge para a branch principal.
5. O workflow `Deploy EC2` builda as imagens no GitHub Actions.
6. O workflow acessa a EC2 por SSH, faz `docker pull` e roda Docker Compose.

## Secrets no GitHub

No repositorio do back-end, configure em:

`Settings > Secrets and variables > Actions > New repository secret`

Secrets obrigatorios:

```text
EC2_HOST=IP_PUBLICO_DA_EC2
EC2_USER=ubuntu
EC2_SSH_KEY=conteudo_da_chave_privada_pem
EC2_APP_DIR=/home/ubuntu/apps
FRONTEND_REPO_TOKEN=token_do_github_com_leitura_no_repo_do_front
GHCR_READ_TOKEN=token_do_github_com_read_packages
GHCR_USERNAME=seu_usuario_github
```

`FRONTEND_REPO_TOKEN` e `GHCR_READ_TOKEN` podem ser o mesmo token, desde que tenha permissao de leitura no repositorio do front e permissao `read:packages`.

## Preparar a EC2

Na EC2, crie a pasta base:

```bash
mkdir -p ~/apps
mkdir -p ~/apps/MeuProjetoFinanceiro
```

Como o build acontece no GitHub Actions, a EC2 nao precisa clonar os repositorios para o deploy por imagem.

## Arquivo `.env` na EC2

Crie o `.env` somente na EC2:

```bash
cd ~/apps/MeuProjetoFinanceiro
cp .env.example .env
nano .env
```

Exemplo para SQLite:

```bash
EC2_CONNECTION_STRING=Data Source=/data/financeiro.db
EC2_DATABASE_SCHEMA=
EXPO_PUBLIC_API_URL=/api
API_PORT=8081
FRONTEND_PORT=80

Auth__Username=admin
Auth__Password=SUA_SENHA_FORTE
Auth__JwtKey=SUA_CHAVE_GRANDE_COM_PELO_MENOS_32_CARACTERES
Auth__Issuer=MeuProjetoFinanceiro
Auth__Audience=MeuProjetoFinanceiro.App
```

## Deploy manual na EC2

```bash
cd ~/apps/MeuProjetoFinanceiro
docker compose --env-file .env -f docker-compose.ec2.full.yml up -d --build
```

Para deploy por imagem, usado pelo CI/CD:

```bash
API_IMAGE=ghcr.io/SEU_USUARIO/meu-financeiro-api:latest \
FRONTEND_IMAGE=ghcr.io/SEU_USUARIO/meu-financeiro-frontend:latest \
docker compose --env-file .env -f docker-compose.ec2.images.yml up -d
```

## Deploy manual pelo GitHub

No repositorio do back-end:

`Actions > Deploy EC2 > Run workflow`

## Testes

Front:

```text
http://IP_PUBLICO_DA_EC2
```

API:

```text
http://IP_PUBLICO_DA_EC2/api/auth/login
```
