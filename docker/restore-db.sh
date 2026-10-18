#!/bin/bash
# Restaura una base desde un respaldo de /var/opt/mssql/backup (volumen sqlserver-data).
#   docker compose --profile operacion run --rm -e BASE=DITIC -e ARCHIVO=DITIC_20261018_020000.bak db-restore
# Sin ARCHIVO usa el respaldo más reciente de la base. Reemplaza la base actual: úsalo con cuidado.
set -euo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
DB_HOST="${DB_HOST:-sqlserver}"
BASE="${BASE:?Indica BASE (DITIC, RRHH, DIDE, DEaDV o DAC)}"

run_sql() {
    "$SQLCMD" -S "$DB_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -I "$@"
}

if [ -z "${ARCHIVO:-}" ]; then
    ARCHIVO="$(run_sql -h -1 -W -Q "SET NOCOUNT ON;
        SELECT TOP 1 RIGHT(bmf.physical_device_name, CHARINDEX('/', REVERSE(bmf.physical_device_name)) - 1)
        FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = b.media_set_id
        WHERE b.database_name = N'$BASE' AND b.type = 'D' ORDER BY b.backup_finish_date DESC" | tr -d '[:space:]')"
fi
[ -n "$ARCHIVO" ] || { echo "No hay respaldos de $BASE"; exit 1; }

echo "==> Restaurando $BASE desde $ARCHIVO"
run_sql -Q "ALTER DATABASE [$BASE] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            RESTORE DATABASE [$BASE] FROM DISK = N'/var/opt/mssql/backup/$ARCHIVO' WITH REPLACE, CHECKSUM;
            ALTER DATABASE [$BASE] SET MULTI_USER;"

# El login de la API se vuelve a vincular por si el respaldo viene de otro servidor
run_sql -Q "USE [$BASE]; IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$APP_DB_USER')
            ALTER USER [$APP_DB_USER] WITH LOGIN = [$APP_DB_USER];"
echo "==> $BASE restaurada"
