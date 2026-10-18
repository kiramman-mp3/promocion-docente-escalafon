#!/bin/bash
# Inicializa SQL Server: crea las bases, el login de solo lectura de la API y (opcional) los datos de prueba.
# Todos los scripts son idempotentes, así que se puede volver a ejecutar sin duplicar datos.
set -euo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
DB_HOST="${DB_HOST:-sqlserver}"

run_sql() {
    "$SQLCMD" -S "$DB_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -f 65001 "$@"
}

echo "==> Esperando a que SQL Server acepte conexiones en $DB_HOST..."
for i in $(seq 1 60); do
    if run_sql -Q "SELECT 1" > /dev/null 2>&1; then break; fi
    sleep 2
done

echo "==> Creando bases de datos y tablas"
for f in /database/schema/*.sql; do
    echo "   - $(basename "$f")"
    run_sql -i "$f"
done

echo "==> Creando login de la API ($APP_DB_USER)"
run_sql -i /database/security/01-app-login.sql \
    -v APP_DB_USER="$APP_DB_USER" APP_DB_PASSWORD="$APP_DB_PASSWORD"

if [ "${SEED_DATA:-true}" = "true" ]; then
    echo "==> Cargando datos de prueba"
    for f in /database/seed/*.sql; do
        echo "   - $(basename "$f")"
        run_sql -i "$f"
    done
fi

echo "==> Base de datos lista"
