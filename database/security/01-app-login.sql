/*
  Crea el login de la API con permisos de solo lectura en las 5 bases.
  La API únicamente ejecuta SELECT, así que no necesita usar 'sa'.

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
