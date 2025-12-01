#!/usr/bin/env bash
set -a
source .env
set +a

echo "=== Running EF Core Scaffold ==="

dotnet ef dbcontext scaffold \
  "$CONN_STR" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  --output-dir ./Entities \
  --context-dir . \
  --context MyDbContext \
  --no-onconfiguring \
  --namespace efscaffold.Entities \
  --context-namespace Infrastructure.Postgres.Scaffolding \
  --schema deadpigeons \
  --force

