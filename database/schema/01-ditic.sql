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

-- Requisitos de promoción configurables y con vigencia (CES arts. 76-77, UTA art. 48).
-- Cada fila aplica a la promoción desde GradoOrigen al grado siguiente. Nunca se edita una fila vigente:
-- se cierra (VigenteHasta) y se crea otra, para que cada solicitud conserve las reglas con que se presentó
-- (CES, disposición transitoria novena).
IF OBJECT_ID('dbo.RequisitosPromocion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RequisitosPromocion (
        Id                         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RequisitosPromocion PRIMARY KEY,
        GradoOrigen                INT NOT NULL,
        GradoDestino               INT NOT NULL,
        Normativa                  NVARCHAR(400) NOT NULL,
        Resolucion                 NVARCHAR(150) NOT NULL,
        EsPropuesta                BIT NOT NULL CONSTRAINT DF_RequisitosPromocion_EsPropuesta DEFAULT 0,
        VigenteDesde               DATE NOT NULL,
        VigenteHasta               DATE NULL,
        ObrasTotales               INT NOT NULL,
        ObrasEnGrado               INT NOT NULL,
        EvaluacionMinima           DECIMAL(5,2) NOT NULL,
        EvaluacionPeriodos         INT NOT NULL,
        CapacitacionHoras          INT NOT NULL,
        CapacitacionAnios          INT NOT NULL,
        CapacitacionPctPedagogica  DECIMAL(5,2) NOT NULL,
        ProyectosMeses             INT NOT NULL CONSTRAINT DF_RequisitosPromocion_Proyectos DEFAULT 0,
        RequiereDireccionProyecto  BIT NOT NULL CONSTRAINT DF_RequisitosPromocion_Direccion DEFAULT 0,
        TrabajosTitulacion         INT NOT NULL CONSTRAINT DF_RequisitosPromocion_Titulacion DEFAULT 0,
        TitulacionNivelMinimo      NVARCHAR(20) NOT NULL CONSTRAINT DF_RequisitosPromocion_Nivel DEFAULT N'Grado'
            CONSTRAINT CK_RequisitosPromocion_Nivel CHECK (TitulacionNivelMinimo IN (N'Grado', N'Maestria', N'Doctorado')),
        CreadoPorId                INT NULL CONSTRAINT FK_RequisitosPromocion_CreadoPor REFERENCES dbo.Usuarios (Id),
        FechaCreacion              DATETIME2 NOT NULL CONSTRAINT DF_RequisitosPromocion_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_RequisitosPromocion_Grados CHECK (GradoDestino = GradoOrigen + 1),
        CONSTRAINT CK_RequisitosPromocion_Vigencia CHECK (VigenteHasta IS NULL OR VigenteHasta >= VigenteDesde)
    );
    -- Un solo requisito abierto (sin fecha de fin) por grado de origen
    CREATE UNIQUE INDEX UX_RequisitosPromocion_Abierto
        ON dbo.RequisitosPromocion (GradoOrigen) WHERE VigenteHasta IS NULL;
END
GO

-- Foto de los requisitos con que se presentó cada solicitud
IF COL_LENGTH('dbo.SolicitudesPromocion', 'RequisitosPromocionId') IS NULL
    ALTER TABLE dbo.SolicitudesPromocion ADD RequisitosPromocionId INT NULL
        CONSTRAINT FK_SolicitudesPromocion_Requisitos REFERENCES dbo.RequisitosPromocion (Id);
GO

-- Dictamen calculado al presentar la solicitud (JSON), para conservar la evaluación con la que se presentó
IF COL_LENGTH('dbo.SolicitudesPromocion', 'DictamenJson') IS NULL
    ALTER TABLE dbo.SolicitudesPromocion ADD DictamenJson NVARCHAR(MAX) NULL;
GO

-- Evidencias que anexa el docente a su solicitud (CES art. 99): certificados que no están en las bases institucionales
IF OBJECT_ID('dbo.EvidenciasSolicitud', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EvidenciasSolicitud (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EvidenciasSolicitud PRIMARY KEY,
        SolicitudId   INT NOT NULL CONSTRAINT FK_EvidenciasSolicitud_Solicitud REFERENCES dbo.SolicitudesPromocion (Id),
        Tipo          NVARCHAR(30) NOT NULL
            CONSTRAINT CK_EvidenciasSolicitud_Tipo CHECK (Tipo IN (N'CertificadoCongreso', N'Capacitacion', N'Equivalencia',
                N'Sabatico', N'CargoAutoridad', N'Obra', N'Proyecto', N'Otro')),
        Descripcion   NVARCHAR(300) NOT NULL,
        NombreArchivo NVARCHAR(200) NOT NULL,
        Contenido     VARBINARY(MAX) NOT NULL,
        Hash          CHAR(64) NOT NULL,
        Tamano        INT NOT NULL,
        SubidoPorId   INT NOT NULL CONSTRAINT FK_EvidenciasSolicitud_SubidoPor REFERENCES dbo.Usuarios (Id),
        FechaSubida   DATETIME2 NOT NULL CONSTRAINT DF_EvidenciasSolicitud_Fecha DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_EvidenciasSolicitud_Solicitud ON dbo.EvidenciasSolicitud (SolicitudId);
END
GO

-- Tipos de documento revisables: se agregan proyectos, trabajos de titulación, equivalencias y evidencias
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_RevisionesDocumento_Tipo' AND definition NOT LIKE '%Evidencia%')
BEGIN
    ALTER TABLE dbo.RevisionesDocumento DROP CONSTRAINT CK_RevisionesDocumento_Tipo;
    ALTER TABLE dbo.RevisionesDocumento ADD CONSTRAINT CK_RevisionesDocumento_Tipo CHECK (TipoDocumento IN
        (N'TiempoRol', N'Obra', N'Evaluacion', N'Investigacion', N'Curso', N'Proyecto', N'TrabajoTitulacion', N'Equivalencia', N'Evidencia'));
END
GO
