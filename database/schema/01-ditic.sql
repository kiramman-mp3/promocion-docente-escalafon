USE DITIC;
GO

-- Usado por UsuarioRepository.GetByCorreo (login). Contrasena guarda un hash BCrypt.
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Correo      NVARCHAR(150) NOT NULL CONSTRAINT UQ_Usuarios_Correo UNIQUE,
        Contrasena  NVARCHAR(100) NOT NULL,
        Nombre      NVARCHAR(100) NOT NULL,
        Apellido    NVARCHAR(100) NOT NULL,
        Rol         NVARCHAR(50)  NOT NULL
            CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Administrador', 'RRHH', 'Docente')),
        Cedula      CHAR(10) NULL,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT SYSUTCDATETIME()
    );
END
GO

-- Usuarios desactivados por el administrador no pueden iniciar sesión.
IF COL_LENGTH('dbo.Usuarios', 'Activo') IS NULL
    ALTER TABLE dbo.Usuarios ADD Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT 1;
GO

-- Un docente solo puede tener una solicitud pendiente a la vez (índice filtrado).
IF OBJECT_ID('dbo.SolicitudesPromocion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SolicitudesPromocion (
        Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SolicitudesPromocion PRIMARY KEY,
        Cedula           CHAR(10) NOT NULL,
        RolActualId      INT NOT NULL,
        RolActual        NVARCHAR(50) NOT NULL,
        RolObjetivo      NVARCHAR(50) NOT NULL,
        Estado           NVARCHAR(20) NOT NULL CONSTRAINT DF_SolicitudesPromocion_Estado DEFAULT N'Pendiente'
            CONSTRAINT CK_SolicitudesPromocion_Estado CHECK (Estado IN (N'Pendiente', N'Aprobada', N'Rechazada')),
        SolicitadoPorId  INT NOT NULL CONSTRAINT FK_SolicitudesPromocion_SolicitadoPor REFERENCES dbo.Usuarios (Id),
        FechaSolicitud   DATETIME2 NOT NULL CONSTRAINT DF_SolicitudesPromocion_Fecha DEFAULT SYSUTCDATETIME(),
        ResueltoPorId    INT NULL CONSTRAINT FK_SolicitudesPromocion_ResueltoPor REFERENCES dbo.Usuarios (Id),
        FechaResolucion  DATETIME2 NULL,
        Observacion      NVARCHAR(1000) NULL
    );
    CREATE UNIQUE INDEX UX_SolicitudesPromocion_PendientePorCedula
        ON dbo.SolicitudesPromocion (Cedula) WHERE Estado = N'Pendiente';
    CREATE INDEX IX_SolicitudesPromocion_Estado ON dbo.SolicitudesPromocion (Estado, FechaSolicitud);
END
GO

-- Revisión de cada documento del expediente dentro de una solicitud.
-- DocumentoId es el Id de la fila en la base de origen (RRHH, DIDE o DEaDV) según TipoDocumento.
IF OBJECT_ID('dbo.RevisionesDocumento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RevisionesDocumento (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RevisionesDocumento PRIMARY KEY,
        SolicitudId    INT NOT NULL CONSTRAINT FK_RevisionesDocumento_Solicitud REFERENCES dbo.SolicitudesPromocion (Id),
        TipoDocumento  NVARCHAR(20) NOT NULL
            CONSTRAINT CK_RevisionesDocumento_Tipo CHECK (TipoDocumento IN (N'TiempoRol', N'Obra', N'Evaluacion', N'Investigacion', N'Curso')),
        DocumentoId    INT NOT NULL,
        Estado         NVARCHAR(20) NOT NULL
            CONSTRAINT CK_RevisionesDocumento_Estado CHECK (Estado IN (N'Aprobado', N'Rechazado')),
        Observacion    NVARCHAR(500) NULL,
        RevisadoPorId  INT NOT NULL CONSTRAINT FK_RevisionesDocumento_RevisadoPor REFERENCES dbo.Usuarios (Id),
        FechaRevision  DATETIME2 NOT NULL CONSTRAINT DF_RevisionesDocumento_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_RevisionesDocumento_Documento UNIQUE (SolicitudId, TipoDocumento, DocumentoId)
    );
END
GO
