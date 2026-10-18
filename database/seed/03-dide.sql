USE DIDE;
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

IF NOT EXISTS (SELECT 1 FROM dbo.Obras)
BEGIN
    -- Ana: 4 obras desde que es Agregado 1 (una en inglés).
    INSERT INTO dbo.Obras (Cedula, Titulo, FechaPublicacion, NombreRevista, Idioma, FiliacionUTA, Pdf) VALUES
        ('1801234567', N'Modelos de aprendizaje adaptativo en educación superior', '2020-11-10', N'Revista Ciencia UTA',        N'Español', 1, CAST(REPLACE(@pdf, '{T}', 'Obra - Aprendizaje adaptativo') AS VARBINARY(MAX))),
        ('1801234567', N'Machine learning for student dropout prediction',           '2021-06-22', N'IEEE Latin America Trans.',  N'Inglés',  1, CAST(REPLACE(@pdf, '{T}', 'Obra - Dropout prediction')     AS VARBINARY(MAX))),
        ('1801234567', N'Evaluación por competencias en ingeniería',                 '2022-09-05', N'Revista Ingenius',           N'Español', 1, CAST(REPLACE(@pdf, '{T}', 'Obra - Competencias')           AS VARBINARY(MAX))),
        ('1801234567', N'Analítica del aprendizaje en entornos virtuales',           '2024-02-14', N'Revista Enfoque UTE',        N'Español', 1, CAST(REPLACE(@pdf, '{T}', 'Obra - Analitica')              AS VARBINARY(MAX))),
    -- Carlos: 1 obra.
        ('1809876543', N'Diseño de redes de sensores para agricultura',              '2024-05-30', N'Revista Ciencia UTA',        N'Español', 1, CAST(REPLACE(@pdf, '{T}', 'Obra - Redes de sensores')      AS VARBINARY(MAX)));

    -- Lucía: 16 obras desde 2018 (algunas en inglés).
    DECLARE @i INT = 1;
    WHILE @i <= 16
    BEGIN
        INSERT INTO dbo.Obras (Cedula, Titulo, FechaPublicacion, NombreRevista, Idioma, FiliacionUTA, Pdf)
        VALUES (
            '1712345678',
            CONCAT(N'Biotecnología aplicada - estudio ', @i),
            DATEADD(MONTH, @i * 5, '2018-03-01'),
            CASE WHEN @i % 3 = 0 THEN N'Journal of Applied Microbiology' ELSE N'Revista Alimentos Ciencia e Ingeniería' END,
            CASE WHEN @i % 3 = 0 THEN N'Inglés' ELSE N'Español' END,
            1,
            CAST(REPLACE(@pdf, '{T}', CONCAT('Obra - Biotecnologia ', @i)) AS VARBINARY(MAX))
        );
        SET @i += 1;
    END
END

IF NOT EXISTS (SELECT 1 FROM dbo.Investigaciones)
BEGIN
    INSERT INTO dbo.Investigaciones (Cedula, Nombre, Encargado, Horas, Pdf) VALUES
        ('1801234567', N'Sistema de alerta temprana de deserción estudiantil', N'Ana María Pérez',  80, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Alerta temprana')   AS VARBINARY(MAX))),
        ('1801234567', N'Plataforma de tutorías inteligentes',                 N'Jorge Ruiz',       60, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Tutorias')          AS VARBINARY(MAX))),
        ('1712345678', N'Probióticos a partir de frutas andinas',              N'Lucía Mora',      220, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Probioticos')       AS VARBINARY(MAX))),
        ('1712345678', N'Bioplásticos con residuos agroindustriales',          N'Lucía Mora',      200, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Bioplasticos')      AS VARBINARY(MAX)));
END

IF NOT EXISTS (SELECT 1 FROM dbo.ParticipacionProyecto)
BEGIN
    INSERT INTO dbo.ParticipacionProyecto (Cedula, NombreProyecto, Rol, MesesParticipacion) VALUES
        ('1801234567', N'Sistema de alerta temprana de deserción estudiantil', N'CoordinadorPrincipal',  6),
        ('1801234567', N'Plataforma de tutorías inteligentes',                 N'Investigador',          4),
        ('1809876543', N'Riego inteligente en Tungurahua',                      N'Investigador',          3),
        ('1712345678', N'Probióticos a partir de frutas andinas',              N'CoordinadorPrincipal', 12),
        ('1712345678', N'Bioplásticos con residuos agroindustriales',          N'CoordinadorSubrogante', 8);
END
GO
