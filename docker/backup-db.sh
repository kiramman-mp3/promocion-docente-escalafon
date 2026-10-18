#!/bin/bash
# Respaldo completo y verificado de las bases (Reglamento DITIC art. 23 e).
# Los archivos .bak quedan en /var/opt/mssql/backup, dentro del volumen sqlserver-data de SQL Server.
#   docker compose --profile operacion run --rm db-backup
set -euo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
DB_HOST="${DB_HOST:-sqlserver}"
BASES="${BASES:-DITIC RRHH DIDE DEaDV DAC}"
MARCA="$(date -u +%Y%m%d_%H%M%S)"
RETENCION_DIAS="${RETENCION_DIAS:-30}"

run_sql() {
    "$SQLCMD" -S "$DB_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -I "$@"
}

for base in $BASES; do
    archivo="/var/opt/mssql/backup/${base}_${MARCA}.bak"
    echo "==> Respaldando $base en $archivo"
    run_sql -Q "BACKUP DATABASE [$base] TO DISK = N'$archivo' WITH COMPRESSION, CHECKSUM, INIT, NAME = N'$base respaldo completo $MARCA'"
    echo "    Verificando el respaldo"
    run_sql -Q "RESTORE VERIFYONLY FROM DISK = N'$archivo' WITH CHECKSUM"
done

# Elimina respaldos más antiguos que la retención (se ejecuta dentro del servidor con xp_delete_file)
run_sql -Q "DECLARE @limite DATETIME = DATEADD(DAY, -$RETENCION_DIAS, GETDATE()); EXEC master.dbo.xp_delete_file 0, N'/var/opt/mssql/backup/', N'bak', @limite, 0;" || true

echo "==> Respaldo $MARCA completo"
