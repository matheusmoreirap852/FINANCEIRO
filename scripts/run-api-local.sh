#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if [ -f .env ]; then
  while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in
      ""|\#*) continue ;;
    esac

    key="${line%%=*}"
    value="${line#*=}"
    export "$key=$value"
  done < .env
fi

dotnet run --project MeuProjetoFinanceiro.API --urls http://0.0.0.0:8081
