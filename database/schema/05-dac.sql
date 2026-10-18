USE DAC;
GO

-- Usado por TesisRepository (COUNT(*) por cédula).
IF OBJECT_ID('dbo.TesisDoctorado', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TesisDoctorado (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TesisDoctorado PRIMARY KEY,
        Cedula      CHAR(10) NOT NULL,
        Titulo      NVARCHAR(300) NOT NULL,
        Universidad NVARCHAR(150) NOT NULL,
        EsDirigida  BIT NOT NULL CONSTRAINT DF_TesisDoctorado_EsDirigida DEFAULT 1
    );
    CREATE INDEX IX_TesisDoctorado_Cedula ON dbo.TesisDoctorado (Cedula);
END
GO
