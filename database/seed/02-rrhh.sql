/*
  Escalafón docente (CES art. 63) y datos de RRHH de los docentes de prueba.
  Las fechas que deben conservar su sentido con el paso del tiempo se calculan desde la fecha actual.

  Casos de prueba:
    1801234567 Ana     Agregado 1 desde 2020: cumple todos los requisitos para Agregado 2.
    1809876543 Carlos  Auxiliar 2 desde hace 30 meses: no cumple la permanencia mínima de 4 años.
    1712345678 Lucía   Principal 1 desde 2018: cumple para Principal 2 (3 años de permanencia).
    1805555555 Jorge   Agregado 3: no tiene promoción; Principal 1 se gana por concurso (CES art. 76).
    1701111111 Marta   Agregado 2 desde hace 28 meses, titular desde 2008 (experiencia acumulada,
                       transitoria quinta) y Vicerrectora más de 2 años (excepciones del art. 77 f).
*/
USE RRHH;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
BEGIN
    INSERT INTO dbo.Roles (Id, Nombre, RequiereAniosEnRol, RequiereObras, RequiereEvaluacion, RequiereCapacitacionHoras, RequiereInvestigacionMeses) VALUES
        (1, N'Auxiliar 1',  0, 0, 0, 0, 0),
        (2, N'Auxiliar 2',  0, 0, 0, 0, 0),
        (3, N'Agregado 1',  0, 0, 0, 0, 0),
        (4, N'Agregado 2',  0, 0, 0, 0, 0),
        (5, N'Agregado 3',  0, 0, 0, 0, 0),
        (6, N'Principal 1', 0, 0, 0, 0, 0),
        (7, N'Principal 2', 0, 0, 0, 0, 0),
        (8, N'Principal 3', 0, 0, 0, 0, 0);
END
GO

-- CES art. 63 (tabla de categorías, niveles, grados y duración). Se actualiza en cada ejecución.
UPDATE r SET Categoria = v.Categoria, Nivel = v.Nivel, Grado = v.Grado,
             PermanenciaAnios = v.PermanenciaAnios, Promocionable = v.Promocionable
FROM dbo.Roles r
JOIN (VALUES
    (1, N'Auxiliar',  1, 1, 4,    1),
    (2, N'Auxiliar',  2, 2, 4,    1),
    (3, N'Agregado',  1, 3, 4,    1),
    (4, N'Agregado',  2, 4, 4,    1),
    (5, N'Agregado',  3, 5, 4,    0),
    (6, N'Principal', 1, 6, 3,    1),
    (7, N'Principal', 2, 7, 3,    1),
    (8, N'Principal', 3, 8, NULL, 0)
) AS v(Id, Categoria, Nivel, Grado, PermanenciaAnios, Promocionable) ON v.Id = r.Id;
GO

DECLARE @pdf VARCHAR(MAX) = '%PDF-1.4
1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj
2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj
3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 595 842]/Resources<</Font<</F1 4 0 R>>>>/Contents 5 0 R>>endobj
4 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj
5 0 obj<</Length 160>>stream
BT /F1 18 Tf 60 780 Td ({T}) Tj ET
BT /F1 11 Tf 60 750 Td (Documento de prueba generado para desarrollo - UTA) Tj ET
endstream
endobj
trailer<</Root 1 0 R>>
%%EOF';
DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

IF NOT EXISTS (SELECT 1 FROM dbo.TiempoRol)
BEGIN
    INSERT INTO dbo.TiempoRol (Cedula, RolActual, FechaInicio, Pdf) VALUES
        ('1801234567', N'Agregado 1',  '2020-03-01',                   CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Agregado 1')  AS VARBINARY(MAX))),
        ('1809876543', N'Auxiliar 2',  DATEADD(MONTH, -30, @hoy),      CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Auxiliar 2')  AS VARBINARY(MAX))),
        ('1712345678', N'Principal 1', '2018-01-15',                   CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Principal 1') AS VARBINARY(MAX))),
        ('1805555555', N'Agregado 3',  '2019-08-01',                   CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Agregado 3')  AS VARBINARY(MAX))),
        ('1701111111', N'Agregado 2',  DATEADD(MONTH, -28, @hoy),      CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Agregado 2')  AS VARBINARY(MAX)));
END

IF NOT EXISTS (SELECT 1 FROM dbo.TrayectoriaDocente)
BEGIN
    INSERT INTO dbo.TrayectoriaDocente (Cedula, FechaIngresoInstitucion, FechaIngresoTitular, MesesNoTitular, MesesTecnicoDocente, IngresoConPhD) VALUES
        ('1801234567', '2015-04-01', '2016-03-01',                 12, 0, 0),
        ('1809876543', DATEADD(MONTH, -54, @hoy), DATEADD(MONTH, -54, @hoy), 0, 0, 0),
        ('1712345678', '2005-09-01', '2009-10-01',                 36, 0, 0),
        ('1805555555', '2010-02-01', '2013-06-01',                  0, 0, 0),
        ('1701111111', '2006-03-01', '2008-05-01',                 24, 6, 0);
END

IF NOT EXISTS (SELECT 1 FROM dbo.CargosAutoridad)
BEGIN
    -- Marta fue Vicerrectora Académica 25 meses: exenta de capacitación (art. 76 d) y de publicar en el grado (art. 77 f)
    INSERT INTO dbo.CargosAutoridad (Cedula, Cargo, TipoCargo, FechaInicio, FechaFin, Pdf) VALUES
        ('1701111111', N'Vicerrectora Académica', N'Vicerrector', DATEADD(MONTH, -27, @hoy), DATEADD(MONTH, -2, @hoy),
         CAST(REPLACE(@pdf, '{T}', 'Nombramiento - Vicerrectora Academica') AS VARBINARY(MAX))),
        ('1712345678', N'Directora del Departamento de Investigación', N'AutoridadAcademica', '2015-01-01', '2016-12-31', NULL);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Sabaticos)
BEGIN
    -- Lucía tuvo sabático con informe aprobado: se reduce 25 % la capacitación exigida (art. 77 e)
    INSERT INTO dbo.Sabaticos (Cedula, FechaInicio, FechaFin, InformeAprobado) VALUES
        ('1712345678', DATEADD(MONTH, -20, @hoy), DATEADD(MONTH, -8, @hoy), 1);
END
GO
