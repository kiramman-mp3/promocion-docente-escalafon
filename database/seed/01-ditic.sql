/*
  Datos de desarrollo de DITIC. Todos los usuarios usan la contraseña: Uta2026*
  (hash BCrypt, coste 11). NO usar estos datos en producción. Los nombres son ficticios.
*/
USE DITIC;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios)
BEGIN
    DECLARE @hash NVARCHAR(100) = N'$2a$11$Hcw/sF7PXMQHHEOLjh2OuemySFQAjoxKfA3CK/QzvmqL3yTmqsMTG';

    INSERT INTO dbo.Usuarios (Correo, Contrasena, Nombre, Apellido, Rol, Cedula) VALUES
        (N'admin@uta.edu.ec',           @hash, N'Administrador',  N'Sistema',           N'Administrador', NULL),
        (N'rrhh@uta.edu.ec',            @hash, N'Paola',          N'Sánchez',           N'RRHH',          NULL),
        (N'ana.perez@uta.edu.ec',       @hash, N'Ana María',      N'Pérez Villacís',    N'Docente',       '1801234567'),
        (N'carlos.gomez@uta.edu.ec',    @hash, N'Carlos Andrés',  N'Gómez Lalama',      N'Docente',       '1809876543'),
        (N'lucia.mora@uta.edu.ec',      @hash, N'Lucía Fernanda', N'Mora Andrade',      N'Docente',       '1712345678'),
        (N'jorge.ruiz@uta.edu.ec',      @hash, N'Jorge Patricio', N'Ruiz Naranjo',      N'Docente',       '1805555555'),
        (N'marta.villacres@uta.edu.ec', @hash, N'Marta Cecilia',  N'Villacrés Paredes', N'Docente',       '1701111111'),
        (N'vicerrectorado@uta.edu.ec',  @hash, N'Fernando',       N'Castillo Mena',     N'Autoridad',     NULL),
        (N'financiero@uta.edu.ec',      @hash, N'Gabriela',       N'Torres Vaca',       N'Autoridad',     NULL),
        (N'profesor.hcu@uta.edu.ec',    @hash, N'Diego',          N'Salazar Ortiz',     N'Docente',       '1802222222'),
        (N'profesor.terna@uta.edu.ec',  @hash, N'Verónica',       N'Andrade Lara',      N'Docente',       '1803333333');
END
GO

/*
  Requisitos de promoción PROPUESTOS (EsPropuesta = 1) mientras el Vicerrectorado expide la normativa
  interna de promoción (UTA, disposición transitoria cuarta, 0153-HCU-2026).
  - Mínimos del CES art. 76: 75 % en las 4 últimas evaluaciones; 96 h en 3 años con 25 % pedagógicas;
    al menos 1 obra en el grado actual.
  - Obras totales crecientes entre grados consecutivos, sin superar las 6 del ingreso a Principal 1
    en las promociones de Agregado y con ese ingreso como referencia en Principal (art. 76, incisos finales).
  - Requisitos opcionales de la UTA (art. 48 e): 12 meses en proyectos, dirección de proyectos y
    dirección de trabajos de titulación, crecientes por grado.
*/
IF NOT EXISTS (SELECT 1 FROM dbo.RequisitosPromocion)
BEGIN
    DECLARE @admin INT = (SELECT Id FROM dbo.Usuarios WHERE Correo = N'admin@uta.edu.ec');
    DECLARE @normativa NVARCHAR(400) = N'CES RPC-SE-19-No.055-2021, arts. 76 y 77; UTA 0153-HCU-2026, art. 48';
    DECLARE @resolucion NVARCHAR(150) = N'Propuesta pendiente de la normativa interna de promoción (UTA, transitoria cuarta)';

    INSERT INTO dbo.RequisitosPromocion
        (GradoOrigen, GradoDestino, Normativa, Resolucion, EsPropuesta, VigenteDesde,
         ObrasTotales, ObrasEnGrado, EvaluacionMinima, EvaluacionPeriodos,
         CapacitacionHoras, CapacitacionAnios, CapacitacionPctPedagogica,
         ProyectosMeses, RequiereDireccionProyecto, TrabajosTitulacion, TitulacionNivelMinimo, CreadoPorId)
    VALUES
        (1, 2, @normativa, @resolucion, 1, '2026-03-26', 1, 1, 75, 4, 96, 3, 25,  0, 0, 0, N'Grado',    @admin),
        (2, 3, @normativa, @resolucion, 1, '2026-03-26', 2, 1, 75, 4, 96, 3, 25, 12, 0, 0, N'Grado',    @admin),
        (3, 4, @normativa, @resolucion, 1, '2026-03-26', 3, 1, 75, 4, 96, 3, 25, 12, 0, 1, N'Grado',    @admin),
        (4, 5, @normativa, @resolucion, 1, '2026-03-26', 4, 1, 75, 4, 96, 3, 25, 12, 1, 2, N'Grado',    @admin),
        (6, 7, @normativa, @resolucion, 1, '2026-03-26', 7, 1, 75, 4, 96, 3, 25, 12, 1, 3, N'Maestria', @admin),
        (7, 8, @normativa, @resolucion, 1, '2026-03-26', 8, 1, 75, 4, 96, 3, 25, 12, 1, 3, N'Maestria', @admin);
END
GO

-- Comisión Académica de promoción (UTA art. 47). Lucía integra la comisión: no puede votar su propia solicitud.
IF NOT EXISTS (SELECT 1 FROM dbo.ComisionMiembros)
BEGIN
    INSERT INTO dbo.ComisionMiembros (UsuarioId, Cargo, EsPresidente, VigenteDesde)
    SELECT u.Id, v.Cargo, v.EsPresidente, '2026-04-01'
    FROM (VALUES
        (N'vicerrectorado@uta.edu.ec', N'VicerrectorAcademico',  1),
        (N'profesor.hcu@uta.edu.ec',   N'ProfesorHCU',           0),
        (N'lucia.mora@uta.edu.ec',     N'ProfesorTerna',         0),
        (N'profesor.terna@uta.edu.ec', N'ProfesorTerna',         0),
        (N'financiero@uta.edu.ec',     N'DirectorFinanciero',    0),
        (N'rrhh@uta.edu.ec',           N'DirectorTalentoHumano', 0)
    ) AS v(Correo, Cargo, EsPresidente)
    JOIN dbo.Usuarios u ON u.Correo = v.Correo;
END
GO

-- Ana tiene una solicitud en revisión documental para que Talento Humano la revise
IF NOT EXISTS (SELECT 1 FROM dbo.SolicitudesPromocion)
BEGIN
    INSERT INTO dbo.SolicitudesPromocion (Cedula, RolActualId, RolActual, RolObjetivo, Estado, SolicitadoPorId, RequisitosPromocionId)
    SELECT '1801234567', 3, N'Agregado 1', N'Agregado 2', N'RevisionDocumental', u.Id,
           (SELECT Id FROM dbo.RequisitosPromocion WHERE GradoOrigen = 3 AND VigenteHasta IS NULL)
    FROM dbo.Usuarios u WHERE u.Correo = N'ana.perez@uta.edu.ec';
END
GO
