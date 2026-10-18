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
DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

/*
  Obras con los criterios de relevancia del CES (arts. 78-87):
  - Artículo arbitrado: revisión por pares, ISSN e índice científico.
  - Libro y capítulo: revisión por pares e ISBN/ISSN.
  - Ponencia: ISBN o memoria indexada, y revisión por pares.
  - Propiedad industrial, software o prototipo: registro en el SENADI o valoración externa.
*/
IF NOT EXISTS (SELECT 1 FROM dbo.Obras)
BEGIN
    INSERT INTO dbo.Obras (Cedula, Titulo, FechaPublicacion, NombreRevista, Idioma, FiliacionUTA, TipoObra, ISSN, ISBN, BaseIndexacion, RevisionPares, RegistroSENADI, ValoracionExterna, Pdf) VALUES
    -- Ana: 5 obras relevantes (4 durante Agregado 1) y 1 sin indexación, que no cuenta
        ('1801234567', N'Rendimiento académico y uso de plataformas LMS', '2018-06-15', N'Revista Ciencia UTA', N'Español', 1, N'ArticuloArbitrado', N'1390-0001', NULL, N'Latindex Catálogo', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Rendimiento LMS') AS VARBINARY(MAX))),
        ('1801234567', N'Modelos de aprendizaje adaptativo en educación superior', '2020-11-10', N'Revista Ingenius', N'Español', 1, N'ArticuloArbitrado', N'1390-650X', NULL, N'SciELO', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Aprendizaje adaptativo') AS VARBINARY(MAX))),
        ('1801234567', N'Machine learning for student dropout prediction', '2021-06-22', N'IEEE Latin America Transactions', N'Inglés', 1, N'ArticuloArbitrado', N'1548-0992', NULL, N'Scopus', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Dropout prediction') AS VARBINARY(MAX))),
        ('1801234567', N'Evaluación por competencias en ingeniería', '2022-09-05', N'Innovación educativa (Editorial UTA)', N'Español', 1, N'CapituloLibro', NULL, N'978-9942-000-01-0', NULL, 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Competencias') AS VARBINARY(MAX))),
        ('1801234567', N'Analítica del aprendizaje en entornos virtuales', '2024-02-14', N'Actas CITIS 2024', N'Español', 1, N'Ponencia', NULL, N'978-3-031-00000-1', N'Scopus', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Analitica') AS VARBINARY(MAX))),
        ('1801234567', N'Uso de chatbots en tutorías', '2023-05-03', N'Boletín de la Facultad', N'Español', 1, N'ArticuloArbitrado', NULL, NULL, NULL, 0, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Chatbots') AS VARBINARY(MAX))),
    -- Carlos: 1 obra relevante
        ('1809876543', N'Diseño de redes de sensores para agricultura', DATEADD(MONTH, -10, @hoy), N'Revista Ciencia UTA', N'Español', 1, N'ArticuloArbitrado', N'1390-0001', NULL, N'Latindex Catálogo', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Redes de sensores') AS VARBINARY(MAX))),
    -- Jorge: 3 obras relevantes
        ('1805555555', N'Control predictivo de motores eléctricos', '2016-04-20', N'Revista Técnica Energía', N'Español', 1, N'ArticuloArbitrado', N'1390-5074', NULL, N'Latindex Catálogo', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Control predictivo') AS VARBINARY(MAX))),
        ('1805555555', N'Smart grids in Andean cities', '2020-09-11', N'Energies', N'Inglés', 1, N'ArticuloArbitrado', N'1996-1073', NULL, N'Scopus', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Smart grids') AS VARBINARY(MAX))),
        ('1805555555', N'Máquinas eléctricas (libro de texto)', '2022-03-01', N'Editorial UTA', N'Español', 1, N'Libro', NULL, N'978-9942-000-02-7', NULL, 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Maquinas electricas') AS VARBINARY(MAX))),
    -- Marta: 4 obras relevantes, todas antes de su grado actual (exenta de publicar en el grado por el art. 77 f)
        ('1701111111', N'Didáctica de las ciencias sociales', '2015-07-01', N'Editorial Abya-Yala', N'Español', 1, N'Libro', NULL, N'978-9942-09-111-1', NULL, 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Didactica sociales') AS VARBINARY(MAX))),
        ('1701111111', N'Brecha digital en zonas rurales de Tungurahua', '2017-10-12', N'Revista Retos', N'Español', 1, N'ArticuloArbitrado', N'1390-6291', NULL, N'Scopus', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Brecha digital') AS VARBINARY(MAX))),
        ('1701111111', N'Rural teachers and ICT adoption', '2019-05-30', N'Education and Information Technologies', N'Inglés', 1, N'ArticuloArbitrado', N'1360-2357', NULL, N'Scopus', 1, NULL, 0, CAST(REPLACE(@pdf, '{T}', 'Obra - ICT adoption') AS VARBINARY(MAX))),
        ('1701111111', N'Plataforma de alfabetización digital (software)', '2021-08-20', N'Registro SENADI', N'Español', 1, N'DisenoSoftwarePrototipo', NULL, NULL, NULL, 0, N'SENADI-2021-0045', 0, CAST(REPLACE(@pdf, '{T}', 'Obra - Software alfabetizacion') AS VARBINARY(MAX)));

    -- Lucía: 16 obras relevantes entre 2014 y 2024 (algunas en inglés e indexadas en Scopus)
    DECLARE @i INT = 1;
    WHILE @i <= 16
    BEGIN
        INSERT INTO dbo.Obras (Cedula, Titulo, FechaPublicacion, NombreRevista, Idioma, FiliacionUTA, TipoObra, ISSN, ISBN, BaseIndexacion, RevisionPares, RegistroSENADI, ValoracionExterna, Pdf)
        VALUES (
            '1712345678',
            CONCAT(N'Biotecnología aplicada - estudio ', @i),
            DATEADD(MONTH, @i * 8, '2014-03-01'),
            CASE WHEN @i % 3 = 0 THEN N'Journal of Applied Microbiology' ELSE N'Revista Alimentos Ciencia e Ingeniería' END,
            CASE WHEN @i % 3 = 0 THEN N'Inglés' ELSE N'Español' END,
            1,
            N'ArticuloArbitrado',
            CASE WHEN @i % 3 = 0 THEN N'1364-5072' ELSE N'1390-2180' END,
            NULL,
            CASE WHEN @i % 3 = 0 THEN N'Scopus' ELSE N'Latindex Catálogo' END,
            1, NULL, 0,
            CAST(REPLACE(@pdf, '{T}', CONCAT('Obra - Biotecnologia ', @i)) AS VARBINARY(MAX))
        );
        SET @i += 1;
    END
END

IF NOT EXISTS (SELECT 1 FROM dbo.Investigaciones)
BEGIN
    INSERT INTO dbo.Investigaciones (Cedula, Nombre, Encargado, Horas, Pdf) VALUES
        ('1801234567', N'Sistema de alerta temprana de deserción estudiantil', N'Ana María Pérez',  80, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Alerta temprana') AS VARBINARY(MAX))),
        ('1801234567', N'Plataforma de tutorías inteligentes',                 N'Jorge Ruiz',       60, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Tutorias')        AS VARBINARY(MAX))),
        ('1712345678', N'Probióticos a partir de frutas andinas',              N'Lucía Mora',      220, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Probioticos')     AS VARBINARY(MAX))),
        ('1712345678', N'Bioplásticos con residuos agroindustriales',          N'Lucía Mora',      200, CAST(REPLACE(@pdf, '{T}', 'Investigacion - Bioplasticos')    AS VARBINARY(MAX)));
END

-- Proyectos de investigación y vinculación (UTA art. 48 e.1 y e.2)
IF NOT EXISTS (SELECT 1 FROM dbo.ParticipacionProyecto)
BEGIN
    INSERT INTO dbo.ParticipacionProyecto (Cedula, NombreProyecto, Rol, MesesParticipacion, TipoProyecto, FechaInicio, FechaFin) VALUES
        ('1801234567', N'Sistema de alerta temprana de deserción estudiantil', N'Director',     18, N'Investigacion', '2021-01-01', '2022-06-30'),
        ('1801234567', N'Plataforma de tutorías inteligentes',                 N'Participante', 10, N'Vinculacion',   '2023-01-01', '2023-10-31'),
        ('1809876543', N'Riego inteligente en Tungurahua',                     N'Participante',  3, N'Vinculacion',   DATEADD(MONTH, -9, @hoy), DATEADD(MONTH, -6, @hoy)),
        ('1712345678', N'Probióticos a partir de frutas andinas',              N'Director',     12, N'Investigacion', '2019-01-01', '2019-12-31'),
        ('1712345678', N'Bioplásticos con residuos agroindustriales',          N'Codirector',    8, N'Vinculacion',   '2021-03-01', '2021-10-31'),
        ('1805555555', N'Microrredes para comunidades rurales',                N'Participante', 24, N'Investigacion', '2020-01-01', '2021-12-31'),
        ('1701111111', N'Alfabetización digital en comunidades rurales',       N'Director',     14, N'Vinculacion',   '2018-02-01', '2019-03-31');
END
GO
