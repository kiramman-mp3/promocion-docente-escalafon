USE DAC;
GO

-- Trabajos de titulación dirigidos o codirigidos de grado, maestría y doctorado (UTA art. 48 e.3).
-- Reemplaza a TesisDoctorado, que solo registraba doctorados.
IF OBJECT_ID('dbo.TrabajosTitulacionDirigidos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrabajosTitulacionDirigidos (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrabajosTitulacion PRIMARY KEY,
        Cedula          CHAR(10) NOT NULL,
        Titulo          NVARCHAR(300) NOT NULL,
        Nivel           NVARCHAR(20) NOT NULL
            CONSTRAINT CK_TrabajosTitulacion_Nivel CHECK (Nivel IN (N'Grado', N'Maestria', N'Doctorado')),
        Rol             NVARCHAR(20) NOT NULL CONSTRAINT DF_TrabajosTitulacion_Rol DEFAULT N'Director'
            CONSTRAINT CK_TrabajosTitulacion_Rol CHECK (Rol IN (N'Director', N'Codirector')),
        FechaAprobacion DATE NOT NULL,
        Universidad     NVARCHAR(150) NOT NULL,
        Pdf             VARBINARY(MAX) NULL
    );
    CREATE INDEX IX_TrabajosTitulacion_Cedula ON dbo.TrabajosTitulacionDirigidos (Cedula);
END
GO

-- Migra los registros de la tabla anterior y la elimina
IF OBJECT_ID('dbo.TesisDoctorado', 'U') IS NOT NULL
BEGIN
    EXEC (N'INSERT INTO dbo.TrabajosTitulacionDirigidos (Cedula, Titulo, Nivel, Rol, FechaAprobacion, Universidad)
          SELECT Cedula, Titulo, N''Doctorado'', N''Director'', ''2020-01-01'', Universidad FROM dbo.TesisDoctorado;');
    DROP TABLE dbo.TesisDoctorado;
END
GO
