USE RRHH;
GO

-- Catálogo del escalafón. RolRepository.ObtenerSiguienteRol toma el primer Id mayor al actual,
-- por eso los Id deben seguir el orden de la carrera docente.
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        Id                          INT NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        Nombre                      NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_Nombre UNIQUE,
        RequiereAniosEnRol          INT NOT NULL CONSTRAINT DF_Roles_Anios DEFAULT 0,
        RequiereObras               INT NOT NULL CONSTRAINT DF_Roles_Obras DEFAULT 0,
        RequiereEvaluacion          DECIMAL(5,2) NOT NULL CONSTRAINT DF_Roles_Eval DEFAULT 0,
        RequiereCapacitacionHoras   INT NOT NULL CONSTRAINT DF_Roles_Cap DEFAULT 0,
        RequiereInvestigacionMeses  INT NOT NULL CONSTRAINT DF_Roles_Inv DEFAULT 0
    );
END
GO

-- Usado por RRHHRepository.ObtenerTiempoRolPorCedulaAsync. Pdf = acción de personal.
IF OBJECT_ID('dbo.TiempoRol', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TiempoRol (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TiempoRol PRIMARY KEY,
        Cedula      CHAR(10) NOT NULL CONSTRAINT UQ_TiempoRol_Cedula UNIQUE,
        RolActual   NVARCHAR(50) NOT NULL,
        FechaInicio DATE NOT NULL,
        Pdf         VARBINARY(MAX) NOT NULL
    );
END
GO

-- CES art. 63: categoría, nivel, grado escalafonario y permanencia mínima en el grado.
-- Promocionable = 0 en los grados 5 (Agregado 3) y 8 (Principal 3): el art. 76 excluye al grado 5
-- (Principal 1 se alcanza por concurso de méritos y oposición) y el 8 es el último grado.
IF COL_LENGTH('dbo.Roles', 'Categoria') IS NULL
    ALTER TABLE dbo.Roles ADD
        Categoria        NVARCHAR(20) NULL,
        Nivel            INT NULL,
        Grado            INT NULL,
        PermanenciaAnios INT NULL,
        Promocionable    BIT NOT NULL CONSTRAINT DF_Roles_Promocionable DEFAULT 0;
GO

-- Trayectoria docente: necesaria para la experiencia acumulada del personal que ingresó antes del
-- 07-11-2012 (UTA 0153-HCU-2026, transitoria quinta; CES, transitoria décima primera).
IF OBJECT_ID('dbo.TrayectoriaDocente', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrayectoriaDocente (
        Cedula                  CHAR(10) NOT NULL CONSTRAINT PK_TrayectoriaDocente PRIMARY KEY,
        FechaIngresoInstitucion DATE NOT NULL,
        FechaIngresoTitular     DATE NOT NULL,
        MesesNoTitular          INT NOT NULL CONSTRAINT DF_Trayectoria_NoTitular DEFAULT 0,
        MesesTecnicoDocente     INT NOT NULL CONSTRAINT DF_Trayectoria_Tecnico DEFAULT 0,
        IngresoConPhD           BIT NOT NULL CONSTRAINT DF_Trayectoria_PhD DEFAULT 0
    );
END
GO

-- Cargos de autoridad: excepciones de los arts. 76 d y 77 f del CES
IF OBJECT_ID('dbo.CargosAutoridad', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CargosAutoridad (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CargosAutoridad PRIMARY KEY,
        Cedula      CHAR(10) NOT NULL,
        Cargo       NVARCHAR(150) NOT NULL,
        TipoCargo   NVARCHAR(30) NOT NULL
            CONSTRAINT CK_CargosAutoridad_Tipo CHECK (TipoCargo IN (N'Rector', N'Vicerrector', N'AutoridadOrganismoSES', N'AutoridadAcademica')),
        FechaInicio DATE NOT NULL,
        FechaFin    DATE NULL,
        Pdf         VARBINARY(MAX) NULL
    );
    CREATE INDEX IX_CargosAutoridad_Cedula ON dbo.CargosAutoridad (Cedula);
END
GO

-- Periodo sabático con informe aprobado: excepción del art. 77 e del CES
IF OBJECT_ID('dbo.Sabaticos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sabaticos (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sabaticos PRIMARY KEY,
        Cedula          CHAR(10) NOT NULL,
        FechaInicio     DATE NOT NULL,
        FechaFin        DATE NOT NULL,
        InformeAprobado BIT NOT NULL CONSTRAINT DF_Sabaticos_Informe DEFAULT 0
    );
    CREATE INDEX IX_Sabaticos_Cedula ON dbo.Sabaticos (Cedula);
END
GO
