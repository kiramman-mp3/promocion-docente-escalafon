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

-- Usado por ProyectoPonderadoRepository. Rol pondera los meses: Director x2, Codirector x1,5, Participante x1
-- (CES art. 36 c.2).
IF OBJECT_ID('dbo.ParticipacionProyecto', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ParticipacionProyecto (
        Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParticipacionProyecto PRIMARY KEY,
        Cedula             CHAR(10) NOT NULL,
        NombreProyecto     NVARCHAR(300) NULL,
        Rol                NVARCHAR(30) NOT NULL
            CONSTRAINT CK_ParticipacionProyecto_Rol CHECK (Rol IN (N'Director', N'Codirector', N'Participante')),
        MesesParticipacion INT NOT NULL CONSTRAINT CK_ParticipacionProyecto_Meses CHECK (MesesParticipacion >= 0)
    );
    CREATE INDEX IX_ParticipacionProyecto_Cedula ON dbo.ParticipacionProyecto (Cedula);
END
GO

-- Criterios de obra de relevancia (CES arts. 78-87)
IF COL_LENGTH('dbo.Obras', 'TipoObra') IS NULL
    ALTER TABLE dbo.Obras ADD
        TipoObra          NVARCHAR(30) NOT NULL CONSTRAINT DF_Obras_TipoObra DEFAULT N'ArticuloArbitrado'
            CONSTRAINT CK_Obras_TipoObra CHECK (TipoObra IN (N'ArticuloArbitrado', N'Libro', N'CapituloLibro', N'Ponencia',
                N'PropiedadIndustrial', N'ProduccionArtistica', N'DisenoSoftwarePrototipo', N'Otra')),
        ISSN              NVARCHAR(20) NULL,
        ISBN              NVARCHAR(20) NULL,
        BaseIndexacion    NVARCHAR(100) NULL,
        RevisionPares     BIT NOT NULL CONSTRAINT DF_Obras_RevisionPares DEFAULT 0,
        RegistroSENADI    NVARCHAR(50) NULL,
        ValoracionExterna BIT NOT NULL CONSTRAINT DF_Obras_ValoracionExterna DEFAULT 0;
GO

-- Proyectos de investigación y de vinculación; roles alineados con la norma (dirección x2, codirección x1,5)
IF COL_LENGTH('dbo.ParticipacionProyecto', 'TipoProyecto') IS NULL
    ALTER TABLE dbo.ParticipacionProyecto ADD
        TipoProyecto NVARCHAR(20) NOT NULL CONSTRAINT DF_ParticipacionProyecto_Tipo DEFAULT N'Investigacion'
            CONSTRAINT CK_ParticipacionProyecto_Tipo CHECK (TipoProyecto IN (N'Investigacion', N'Vinculacion')),
        FechaInicio  DATE NULL,
        FechaFin     DATE NULL;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE name = 'CK_ParticipacionProyecto_Rol' AND definition LIKE '%CoordinadorPrincipal%')
BEGIN
    ALTER TABLE dbo.ParticipacionProyecto DROP CONSTRAINT CK_ParticipacionProyecto_Rol;
    EXEC (N'UPDATE dbo.ParticipacionProyecto SET Rol = CASE Rol
        WHEN N''CoordinadorPrincipal'' THEN N''Director''
        WHEN N''CoordinadorSubrogante'' THEN N''Codirector''
        ELSE N''Participante'' END;');
    ALTER TABLE dbo.ParticipacionProyecto ADD CONSTRAINT CK_ParticipacionProyecto_Rol
        CHECK (Rol IN (N'Director', N'Codirector', N'Participante'));
END
GO
