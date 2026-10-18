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

-- Capacitación con horas reales y tipo (CES art. 76 d: 96 h en 3 años, 25 % pedagógicas o congresos internacionales)
IF COL_LENGTH('dbo.CursosCapacitacion', 'Horas') IS NULL
    ALTER TABLE dbo.CursosCapacitacion ADD
        Horas            INT NULL CONSTRAINT CK_CursosCapacitacion_Horas CHECK (Horas >= 0),
        TipoCapacitacion NVARCHAR(30) NOT NULL CONSTRAINT DF_CursosCapacitacion_Tipo DEFAULT N'Cientifica'
            CONSTRAINT CK_CursosCapacitacion_Tipo CHECK (TipoCapacitacion IN (N'Pedagogica', N'Cientifica', N'CongresoInternacional')),
        FechaInicio      DATE NULL,
        FechaFin         DATE NULL;
GO

-- Evaluación por periodo (PEPI) y si en ese periodo ejerció la docencia (CES art. 76 c)
IF COL_LENGTH('dbo.EvaluacionesDocentes', 'PeriodoAcademico') IS NULL
    ALTER TABLE dbo.EvaluacionesDocentes ADD
        PeriodoAcademico NVARCHAR(30) NULL,
        EjercioDocencia  BIT NOT NULL CONSTRAINT DF_EvaluacionesDocentes_Docencia DEFAULT 1;
GO

-- Equivalencias en horas de capacitación o en dirección de titulación (CES art. 77 g, h, i, j)
IF OBJECT_ID('dbo.Equivalencias', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Equivalencias (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Equivalencias PRIMARY KEY,
        Cedula      CHAR(10) NOT NULL,
        Tipo        NVARCHAR(40) NOT NULL
            CONSTRAINT CK_Equivalencias_Tipo CHECK (Tipo IN (N'EvaluadorExternoConcurso', N'ParEvaluadorProyectos',
                N'FacilitadorCESGradoMaestria', N'FacilitadorCESDoctorado', N'FacilitadorCACES', N'EvaluacionAcademicaCES')),
        Fecha       DATE NOT NULL,
        Meses       INT NULL,
        Descripcion NVARCHAR(300) NOT NULL,
        Pdf         VARBINARY(MAX) NULL
    );
    CREATE INDEX IX_Equivalencias_Cedula ON dbo.Equivalencias (Cedula);
END
GO
