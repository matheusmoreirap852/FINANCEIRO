# Autenticacao

A API usa Bearer JWT para proteger os endpoints. O fluxo e:

1. O front chama `POST /api/auth/login` com `username` e `password`.
2. A API valida contra as variaveis de ambiente.
3. A API retorna `accessToken`.
4. O front envia `Authorization: Bearer <token>` nas proximas chamadas.

## Variaveis da API

```bash
Auth__Username=admin
Auth__Password=troque-esta-senha
Auth__JwtKey=troque-esta-chave-com-pelo-menos-32-caracteres
Auth__Issuer=MeuProjetoFinanceiro
Auth__Audience=MeuProjetoFinanceiro.App
```

Use uma senha forte e uma `JwtKey` longa na EC2.

## Teste local

```bash
curl -i http://localhost:8081/api/dashboard
```

Sem token, a resposta esperada e `401 Unauthorized`.

```bash
curl -sS \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"SUA_SENHA"}' \
  http://localhost:8081/api/auth/login
```

Depois use o `accessToken` retornado:

```bash
curl -i \
  -H "Authorization: Bearer SEU_TOKEN" \
  http://localhost:8081/api/dashboard
```
