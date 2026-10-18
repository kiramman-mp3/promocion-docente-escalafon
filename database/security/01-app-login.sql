/*
  Crea el login de la API: lectura en las 5 bases y escritura solo en las tablas del sistema (DITIC).
  La API no usa 'sa': solo tiene los permisos mínimos que necesita.

  Variables sqlcmd (las pasa docker/init-db.sh desde el .env):
    $(APP_DB_USER)      nombre del login
    $(APP_DB_PASSWORD)  contraseña del login
*/
USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(APP_DB_USER)')
    CREATE LOGIN [$(APP_DB_USER)] WITH PASSWORD = N'$(APP_DB_PASSWORD)', CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [$(APP_DB_USER)] WITH PASSWORD = N'$(APP_DB_PASSWORD)';
GO

DECLARE @db SYSNAME, @sql NVARCHAR(MAX);
DECLARE dbs CURSOR LOCAL FAST_FORWARD FOR
    SELECT name FROM sys.databases WHERE name IN (N'DITIC', N'RRHH', N'DIDE', N'DEaDV', N'DAC');
OPEN dbs;
FETCH NEXT FROM dbs INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'USE ' + QUOTENAME(@db) + N';
        IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N''$(APP_DB_USER)'')
            CREATE USER [$(APP_DB_USER)] FOR LOGIN [$(APP_DB_USER)];
        ALTER ROLE db_datareader ADD MEMBER [$(APP_DB_USER)];';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM dbs INTO @db;
END
CLOSE dbs;
DEALLOCATE dbs;
GO

-- Escritura solo en las tablas propias del sistema (DITIC): usuarios y flujo de aprobación.
-- Las bases de las otras direcciones siguen siendo de solo lectura.
USE DITIC;
GO
GRANT INSERT, UPDATE ON dbo.Usuarios TO [$(APP_DB_USER)];
GRANT INSERT, UPDATE ON dbo.SolicitudesPromocion TO [$(APP_DB_USER)];
GRANT INSERT, UPDATE ON dbo.RevisionesDocumento TO [$(APP_DB_USER)];
GRANT INSERT, UPDATE ON dbo.RequisitosPromocion TO [$(APP_DB_USER)];
GRANT INSERT ON dbo.EvidenciasSolicitud TO [$(APP_DB_USER)];
GO
