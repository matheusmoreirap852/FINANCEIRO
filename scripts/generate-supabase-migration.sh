#!/usr/bin/env bash
set -euo pipefail

SCHEMA="${1:-controle_financeiro}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"
python3 scripts/export-sqlite-to-postgres.py \
  --sqlite financeiro.db \
  --schema "$SCHEMA" \
  --output "artifacts/supabase-migration-${SCHEMA}.sql"
