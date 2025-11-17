#!/usr/bin/env bash

echo "=== Running EF Core Scaffold on schema deadpigeons ==="

dotnet ef dbcontext scaffold \
  "Host=ep-icy-pond-ag8lk8z2-pooler.c-2.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_iaJlUvteVL87;Ssl Mode=Require;Channel Binding=Require" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  --output-dir ./Entities \
  --context-dir . \
  --context MyDbContext \
  --no-onconfiguring \
  --namespace efscaffold.Entities \
  --context-namespace Infrastructure.Postgres.Scaffolding \
  --schema deadpigeons \
  --force

