USE DIDE;
GO

-- Usado por ObrasRepository (filtra por FechaPublicacion) y ObraIdiomaRepository (Idioma <> 'Español').
IF OBJECT_ID('dbo.Obras', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Obras (
        Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Obras PRIMARY KEY,
        Cedula           CHAR(10) NOT NULL,
        Titulo           NVARCHAR(300) NULL,
        FechaPublicacion DATE NOT NULL,
        NombreRevista    NVARCHAR(200) NOT NULL,
        Idioma           NVARCHAR(30) NOT NULL CONSTRAINT DF_Obras_Idioma DEFAULT N'Español',
        FiliacionUTA     BIT NOT NULL CONSTRAINT DF_Obras_Filiacion DEFAULT 1,
        Pdf              VARBINARY(MAX) NOT NULL
    );
    CREATE INDEX IX_Obras_Cedula_Fecha ON dbo.Obras (Cedula, FechaPublicacion);
END
GO

-- Usado por InvestigacionRepository.
IF OBJECT_ID('dbo.Investigaciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Investigaciones (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Investigaciones PRIMARY KEY,
        Cedula    CHAR(10) NOT NULL,
        Nombre    NVARCHAR(300) NOT NULL,
        Encargado NVARCHAR(150) NOT NULL,
        Horas     INT NOT NULL CONSTRAINT CK_Investigaciones_Horas CHECK (Horas >= 0),
        Pdf       VARBINARY(MAX) NOT NULL
    );
    CREATE INDEX IX_Investigaciones_Cedula ON dbo.Investigaciones (Cedula);
END
GO

-- Usado por ProyectoPonderadoRepository. Rol pondera los meses:
-- CoordinadorPrincipal x2, CoordinadorSubrogante x1.5, Investigador x1.
IF OBJECT_ID('dbo.ParticipacionProyecto', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ParticipacionProyecto (
        Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParticipacionProyecto PRIMARY KEY,
        Cedula             CHAR(10) NOT NULL,
        NombreProyecto     NVARCHAR(300) NULL,
        Rol                NVARCHAR(30) NOT NULL
            CONSTRAINT CK_ParticipacionProyecto_Rol CHECK (Rol IN ('Investigador', 'CoordinadorPrincipal', 'CoordinadorSubrogante')),
        MesesParticipacion INT NOT NULL CONSTRAINT CK_ParticipacionProyecto_Meses CHECK (MesesParticipacion >= 0)
    );
    CREATE INDEX IX_ParticipacionProyecto_Cedula ON dbo.ParticipacionProyecto (Cedula);
END
GO
