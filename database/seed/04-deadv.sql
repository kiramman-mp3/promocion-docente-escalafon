USE DEaDV;
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

IF NOT EXISTS (SELECT 1 FROM dbo.CursosCapacitacion)
BEGIN
    INSERT INTO dbo.CursosCapacitacion (Cedula, NombreCurso, Universidad, Facultad, FechaEvaluacion, Puntaje) VALUES
        ('1801234567', N'Didáctica universitaria',          N'Universidad Técnica de Ambato', N'Ciencias Humanas y de la Educación', '2021-02-10', 40),
        ('1801234567', N'Ciencia de datos con Python',       N'Universidad Técnica de Ambato', N'Ingeniería en Sistemas',             '2022-07-18', 48),
        ('1801234567', N'Metodología de la investigación',   N'Escuela Politécnica Nacional',  N'Ciencias',                           '2023-11-03', 64),
        ('1809876543', N'Entornos virtuales de aprendizaje', N'Universidad Técnica de Ambato', N'Ingeniería en Sistemas',             '2024-03-12', 40),
        ('1712345678', N'Redacción científica',              N'Universidad Técnica de Ambato', N'Ciencia e Ingeniería en Alimentos',  '2019-05-20', 40),
        ('1712345678', N'Bioestadística avanzada',           N'Universidad de Cuenca',         N'Ciencias Químicas',                  '2020-10-08', 60),
        ('1712345678', N'Gestión de proyectos I+D+i',        N'Universidad Técnica de Ambato', N'Ciencia e Ingeniería en Alimentos',  '2022-04-25', 60);
END

IF NOT EXISTS (SELECT 1 FROM dbo.EvaluacionesDocentes)
BEGIN
    INSERT INTO dbo.EvaluacionesDocentes (Cedula, Periodo, Universidad, Carrera, Puntaje, Pdf) VALUES
        ('1801234567', '2022-03-01', N'Universidad Técnica de Ambato', N'Software', 86.50, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2022-1') AS VARBINARY(MAX))),
        ('1801234567', '2022-09-01', N'Universidad Técnica de Ambato', N'Software', 88.00, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2022-2') AS VARBINARY(MAX))),
        ('1801234567', '2023-03-01', N'Universidad Técnica de Ambato', N'Software', 90.25, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-1') AS VARBINARY(MAX))),
        ('1801234567', '2023-09-01', N'Universidad Técnica de Ambato', N'Software', 89.75, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-2') AS VARBINARY(MAX))),
        ('1801234567', '2024-03-01', N'Universidad Técnica de Ambato', N'Software', 91.00, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-1') AS VARBINARY(MAX))),
        ('1809876543', '2023-09-01', N'Universidad Técnica de Ambato', N'Telecomunicaciones', 72.00, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-2') AS VARBINARY(MAX))),
        ('1809876543', '2024-03-01', N'Universidad Técnica de Ambato', N'Telecomunicaciones', 74.50, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-1') AS VARBINARY(MAX))),
        ('1809876543', '2024-09-01', N'Universidad Técnica de Ambato', N'Telecomunicaciones', 78.00, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-2') AS VARBINARY(MAX))),
        ('1712345678', '2022-03-01', N'Universidad Técnica de Ambato', N'Alimentos', 93.00, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2022-1') AS VARBINARY(MAX))),
        ('1712345678', '2022-09-01', N'Universidad Técnica de Ambato', N'Alimentos', 94.50, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2022-2') AS VARBINARY(MAX))),
        ('1712345678', '2023-03-01', N'Universidad Técnica de Ambato', N'Alimentos', 92.75, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-1') AS VARBINARY(MAX))),
        ('1712345678', '2023-09-01', N'Universidad Técnica de Ambato', N'Alimentos', 95.00, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-2') AS VARBINARY(MAX)));
END

IF NOT EXISTS (SELECT 1 FROM dbo.CapacitacionImpartida)
BEGIN
    INSERT INTO dbo.CapacitacionImpartida (Cedula, NombreCurso, Horas) VALUES
        ('1801234567', N'Introducción a Python para docentes',      32),
        ('1712345678', N'Buenas prácticas de laboratorio',          40),
        ('1712345678', N'Escritura de artículos científicos',       24);
END
GO
