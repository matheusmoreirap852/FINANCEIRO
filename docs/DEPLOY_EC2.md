# Deploy na EC2 com front-end e back-end

Este guia sobe a API C# e o front-end Expo Web em uma EC2 usando Docker Compose.

## Arquitetura

- EC2 Ubuntu LTS
- Docker + Docker Compose
- API C# interna na porta `8081`
- Front-end Nginx na porta `80`, encaminhando `/api` para a API
- SQLite persistido em volume Docker

Para uso pessoal, comece com SQLite no volume Docker. Depois podemos trocar para Supabase/Postgres ou adicionar dominio/HTTPS.

## 1. Criar a EC2

Sugestao inicial:

- AMI: Ubuntu Server LTS
- Tipo: `t3.micro` ou `t3.small`
- Disco: 20 GB ou mais
- Key pair: criar ou usar uma existente

Security Group:

- SSH `22`: somente seu IP
- HTTP `80`: seu IP ou `0.0.0.0/0`
Nao precisa abrir a porta `8081` no Security Group se usar o Nginx com `/api`.

## 2. Instalar Docker

```bash
ssh -i sua-chave.pem ubuntu@IP_PUBLICO_DA_EC2

sudo apt update
sudo apt install -y ca-certificates curl git
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker ubuntu
```

Saia e entre novamente no SSH para o grupo `docker` valer.

## 3. Clonar os dois projetos

Os diretórios precisam ficar lado a lado:

```bash
mkdir -p ~/apps
cd ~/apps

git clone URL_DO_REPOSITORIO_BACKEND MeuProjetoFinanceiro
git clone https://github.com/matheusmoreirap852/front-endProjetoFinanceiro.git MeuProjetoFinanceiro.Frontend
```

## 4. Configurar variaveis

No back-end:

```bash
cd ~/apps/MeuProjetoFinanceiro
cp .env.example .env
nano .env
```

Para SQLite na EC2, deixe assim:

```bash
EC2_CONNECTION_STRING=Data Source=/data/financeiro.db
EC2_DATABASE_SCHEMA=
EXPO_PUBLIC_API_URL=/api
API_PORT=8081
FRONTEND_PORT=80
```

Troque `IP_PUBLICO_DA_EC2` pelo IP publico real.

## 5. Subir front e back

```bash
docker compose --env-file .env -f docker-compose.ec2.full.yml up -d --build
```

Ver logs:

```bash
docker compose -f docker-compose.ec2.full.yml logs -f api
docker compose -f docker-compose.ec2.full.yml logs -f frontend
```

## 6. Testar

Na EC2:

```bash
curl http://localhost:8081/api/dashboard
curl http://localhost
```

No seu computador:

```bash
curl http://IP_PUBLICO_DA_EC2/api/dashboard
```

Abra no navegador:

```text
http://IP_PUBLICO_DA_EC2
```

## Atualizar depois de mudar codigo

```bash
cd ~/apps/MeuProjetoFinanceiro
git pull

cd ~/apps/MeuProjetoFinanceiro.Frontend
git pull

cd ~/apps/MeuProjetoFinanceiro
docker compose --env-file .env -f docker-compose.ec2.full.yml up -d --build
```

## Parar

```bash
docker compose -f docker-compose.ec2.full.yml down
```

## Backup do SQLite

```bash
docker compose -f docker-compose.ec2.full.yml exec api cp /data/financeiro.db /data/financeiro-backup.db
```

Para copiar o backup para sua maquina:

```bash
docker cp $(docker compose -f docker-compose.ec2.full.yml ps -q api):/data/financeiro-backup.db ./financeiro-backup.db
```
