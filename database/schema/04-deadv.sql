USE DEaDV;
GO

-- Usado por CursoRepository. Ojo: PromocionService suma Puntaje como si fueran horas de capacitación.
IF OBJECT_ID('dbo.CursosCapacitacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CursosCapacitacion (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CursosCapacitacion PRIMARY KEY,
        Cedula          CHAR(10) NOT NULL,
        NombreCurso     NVARCHAR(200) NULL,
        Universidad     NVARCHAR(150) NOT NULL,
        Facultad        NVARCHAR(150) NOT NULL,
        FechaEvaluacion DATE NOT NULL,
        Puntaje         DECIMAL(6,2) NOT NULL
    );
    CREATE INDEX IX_CursosCapacitacion_Cedula ON dbo.CursosCapacitacion (Cedula);
END
GO

-- Usado por EvaluacionDocenteRepository. Puntaje en escala 0-100.
IF OBJECT_ID('dbo.EvaluacionesDocentes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EvaluacionesDocentes (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EvaluacionesDocentes PRIMARY KEY,
        Cedula      CHAR(10) NOT NULL,
        Periodo     DATE NOT NULL,
        Universidad NVARCHAR(150) NOT NULL,
        Carrera     NVARCHAR(150) NOT NULL,
        Puntaje     DECIMAL(5,2) NOT NULL CONSTRAINT CK_EvaluacionesDocentes_Puntaje CHECK (Puntaje BETWEEN 0 AND 100),
        Pdf         VARBINARY(MAX) NOT NULL
    );
    CREATE INDEX IX_EvaluacionesDocentes_Cedula ON dbo.EvaluacionesDocentes (Cedula);
END
GO

-- Usado por CapacitacionImpartidaRepository (SUM(Horas)).
IF OBJECT_ID('dbo.CapacitacionImpartida', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CapacitacionImpartida (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CapacitacionImpartida PRIMARY KEY,
        Cedula      CHAR(10) NOT NULL,
        NombreCurso NVARCHAR(200) NULL,
        Horas       INT NOT NULL CONSTRAINT CK_CapacitacionImpartida_Horas CHECK (Horas >= 0)
    );
    CREATE INDEX IX_CapacitacionImpartida_Cedula ON dbo.CapacitacionImpartida (Cedula);
END
GO
