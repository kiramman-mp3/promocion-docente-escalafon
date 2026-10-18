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
