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
