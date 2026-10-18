/*
  Usuarios de desarrollo. Todos usan la contraseña: Uta2026*
  (hash BCrypt, coste 11). NO usar estos datos en producción.
*/
USE DITIC;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios)
BEGIN
    DECLARE @hash NVARCHAR(100) = N'$2a$11$Hcw/sF7PXMQHHEOLjh2OuemySFQAjoxKfA3CK/QzvmqL3yTmqsMTG';

    INSERT INTO dbo.Usuarios (Correo, Contrasena, Nombre, Apellido, Rol, Cedula) VALUES
        (N'admin@uta.edu.ec',        @hash, N'Administrador', N'Sistema',        N'Administrador', NULL),
        (N'rrhh@uta.edu.ec',         @hash, N'Paola',         N'Sánchez',        N'RRHH',          NULL),
        (N'ana.perez@uta.edu.ec',    @hash, N'Ana María',     N'Pérez Villacís', N'Docente',       '1801234567'),
        (N'carlos.gomez@uta.edu.ec', @hash, N'Carlos Andrés', N'Gómez Lalama',   N'Docente',       '1809876543'),
        (N'lucia.mora@uta.edu.ec',   @hash, N'Lucía Fernanda', N'Mora Andrade',  N'Docente',       '1712345678');
END
GO

-- Solicitudes de ejemplo: Ana tiene una pendiente para que RRHH la revise.
IF NOT EXISTS (SELECT 1 FROM dbo.SolicitudesPromocion)
BEGIN
    INSERT INTO dbo.SolicitudesPromocion (Cedula, RolActualId, RolActual, RolObjetivo, SolicitadoPorId)
    SELECT '1801234567', 3, N'Agregado 1', N'Agregado 2', Id
    FROM dbo.Usuarios WHERE Correo = N'ana.perez@uta.edu.ec';
END
GO
