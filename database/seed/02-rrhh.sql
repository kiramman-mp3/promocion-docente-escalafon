/*
  Escalafón docente y tiempo en el rol de los docentes de prueba.
  Los valores de requisitos son ILUSTRATIVOS (basados a grandes rasgos en el Reglamento de
  Carrera y Escalafón del Profesor). Reemplázalos con los valores oficiales de la UTA.
*/
USE RRHH;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
BEGIN
    INSERT INTO dbo.Roles (Id, Nombre, RequiereAniosEnRol, RequiereObras, RequiereEvaluacion, RequiereCapacitacionHoras, RequiereInvestigacionMeses) VALUES
        (1, N'Auxiliar 1',  0,  0,  0.00,   0,  0),
        (2, N'Auxiliar 2',  4,  1, 75.00,  96,  0),
        (3, N'Agregado 1',  4,  2, 75.00,  96,  0),
        (4, N'Agregado 2',  4,  3, 75.00, 128,  6),
        (5, N'Agregado 3',  4,  5, 75.00, 160, 12),
        (6, N'Principal 1', 4, 12, 75.00,  96, 12),
        (7, N'Principal 2', 4, 15, 75.00, 128, 18),
        (8, N'Principal 3', 4, 18, 75.00, 160, 24);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TiempoRol)
BEGIN
    -- PDF mínimo de ejemplo (los visores lo reconstruyen aunque no tenga tabla xref).
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

    INSERT INTO dbo.TiempoRol (Cedula, RolActual, FechaInicio, Pdf) VALUES
        ('1801234567', N'Agregado 1',  '2020-03-01', CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Agregado 1')  AS VARBINARY(MAX))),
        ('1809876543', N'Auxiliar 2',  '2023-09-01', CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Auxiliar 2')  AS VARBINARY(MAX))),
        ('1712345678', N'Principal 1', '2018-01-15', CAST(REPLACE(@pdf, '{T}', 'Accion de personal - Principal 1') AS VARBINARY(MAX)));
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
