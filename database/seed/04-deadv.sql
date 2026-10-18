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
DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

-- Capacitación: las fechas son relativas para que la ventana de 3 años del art. 76 d siga teniendo sentido.
-- Puntaje es la calificación del curso (dato heredado); el requisito se calcula con Horas.
IF NOT EXISTS (SELECT 1 FROM dbo.CursosCapacitacion)
BEGIN
    INSERT INTO dbo.CursosCapacitacion (Cedula, NombreCurso, Universidad, Facultad, FechaEvaluacion, Puntaje, Horas, TipoCapacitacion, FechaInicio, FechaFin) VALUES
    -- Ana: 108 h en los últimos 3 años, 60 h pedagógicas o de congreso internacional; un curso antiguo queda fuera
        ('1801234567', N'Didáctica universitaria',              N'Universidad Técnica de Ambato', N'Ciencias Humanas y de la Educación', DATEADD(MONTH, -6, @hoy),  95, 40, N'Pedagogica',            DATEADD(MONTH, -7, @hoy),  DATEADD(MONTH, -6, @hoy)),
        ('1801234567', N'Ciencia de datos con Python',          N'Universidad Técnica de Ambato', N'Ingeniería en Sistemas',             DATEADD(MONTH, -14, @hoy), 90, 48, N'Cientifica',            DATEADD(MONTH, -15, @hoy), DATEADD(MONTH, -14, @hoy)),
        ('1801234567', N'Congreso Internacional CITIS',         N'Universidad Politécnica Salesiana', N'Ingeniería',                      DATEADD(MONTH, -28, @hoy), 100, 20, N'CongresoInternacional', DATEADD(MONTH, -28, @hoy), DATEADD(MONTH, -28, @hoy)),
        ('1801234567', N'Metodología de la investigación',      N'Escuela Politécnica Nacional',  N'Ciencias',                           DATEADD(MONTH, -44, @hoy), 88, 64, N'Cientifica',            DATEADD(MONTH, -45, @hoy), DATEADD(MONTH, -44, @hoy)),
    -- Carlos: 40 h
        ('1809876543', N'Entornos virtuales de aprendizaje',    N'Universidad Técnica de Ambato', N'Ingeniería en Sistemas',             DATEADD(MONTH, -8, @hoy),  85, 40, N'Cientifica',            DATEADD(MONTH, -9, @hoy),  DATEADD(MONTH, -8, @hoy)),
    -- Lucía: 72 h + 16 h de equivalencia; con sabático se le exige el 75 % (art. 77 e)
        ('1712345678', N'Redacción científica para docentes',   N'Universidad Técnica de Ambato', N'Ciencia e Ingeniería en Alimentos',  DATEADD(MONTH, -5, @hoy),  98, 40, N'Pedagogica',            DATEADD(MONTH, -6, @hoy),  DATEADD(MONTH, -5, @hoy)),
        ('1712345678', N'Bioestadística avanzada',              N'Universidad de Cuenca',         N'Ciencias Químicas',                  DATEADD(MONTH, -18, @hoy), 92, 32, N'Cientifica',            DATEADD(MONTH, -19, @hoy), DATEADD(MONTH, -18, @hoy)),
    -- Jorge
        ('1805555555', N'Sistemas de potencia',                 N'Escuela Politécnica Nacional',  N'Ingeniería Eléctrica',               DATEADD(MONTH, -10, @hoy), 90, 50, N'Cientifica',            DATEADD(MONTH, -11, @hoy), DATEADD(MONTH, -10, @hoy)),
    -- Marta: 20 h (exenta por haber sido autoridad más de 2 años, art. 76 d)
        ('1701111111', N'Liderazgo académico',                  N'Universidad Técnica de Ambato', N'Ciencias Humanas y de la Educación', DATEADD(MONTH, -1, @hoy),  96, 20, N'Pedagogica',            DATEADD(MONTH, -2, @hoy),  DATEADD(MONTH, -1, @hoy));
END

-- Evaluación integral de desempeño por periodo (escala 0-100). EjercioDocencia = 0 en periodos sin carga docente.
IF NOT EXISTS (SELECT 1 FROM dbo.EvaluacionesDocentes)
BEGIN
    INSERT INTO dbo.EvaluacionesDocentes (Cedula, Periodo, PeriodoAcademico, Universidad, Carrera, Puntaje, EjercioDocencia, Pdf) VALUES
        ('1801234567', '2022-03-01', N'PAO 2022-1', N'Universidad Técnica de Ambato', N'Software', 86.50, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2022-1') AS VARBINARY(MAX))),
        ('1801234567', '2022-09-01', N'PAO 2022-2', N'Universidad Técnica de Ambato', N'Software', 88.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2022-2') AS VARBINARY(MAX))),
        ('1801234567', '2023-03-01', N'PAO 2023-1', N'Universidad Técnica de Ambato', N'Software', 90.25, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-1') AS VARBINARY(MAX))),
        ('1801234567', '2023-09-01', N'PAO 2023-2', N'Universidad Técnica de Ambato', N'Software', 89.75, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-2') AS VARBINARY(MAX))),
        ('1801234567', '2024-03-01', N'PAO 2024-1', N'Universidad Técnica de Ambato', N'Software', 91.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-1') AS VARBINARY(MAX))),
        ('1801234567', '2024-09-01', N'PAO 2024-2', N'Universidad Técnica de Ambato', N'Software (comisión de servicios)', 60.00, 0, CAST(REPLACE(@pdf, '{T}', 'Evaluacion 2024-2 sin docencia') AS VARBINARY(MAX))),
        ('1809876543', '2024-03-01', N'PAO 2024-1', N'Universidad Técnica de Ambato', N'Telecomunicaciones', 72.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-1') AS VARBINARY(MAX))),
        ('1809876543', '2024-09-01', N'PAO 2024-2', N'Universidad Técnica de Ambato', N'Telecomunicaciones', 74.50, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-2') AS VARBINARY(MAX))),
        ('1809876543', '2025-03-01', N'PAO 2025-1', N'Universidad Técnica de Ambato', N'Telecomunicaciones', 68.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2025-1') AS VARBINARY(MAX))),
        ('1712345678', '2023-03-01', N'PAO 2023-1', N'Universidad Técnica de Ambato', N'Alimentos', 93.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-1') AS VARBINARY(MAX))),
        ('1712345678', '2023-09-01', N'PAO 2023-2', N'Universidad Técnica de Ambato', N'Alimentos', 94.50, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-2') AS VARBINARY(MAX))),
        ('1712345678', '2024-03-01', N'PAO 2024-1', N'Universidad Técnica de Ambato', N'Alimentos', 92.75, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-1') AS VARBINARY(MAX))),
        ('1712345678', '2025-03-01', N'PAO 2025-1', N'Universidad Técnica de Ambato', N'Alimentos', 95.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2025-1') AS VARBINARY(MAX))),
        ('1805555555', '2023-03-01', N'PAO 2023-1', N'Universidad Técnica de Ambato', N'Electricidad', 85.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-1') AS VARBINARY(MAX))),
        ('1805555555', '2023-09-01', N'PAO 2023-2', N'Universidad Técnica de Ambato', N'Electricidad', 86.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2023-2') AS VARBINARY(MAX))),
        ('1805555555', '2024-03-01', N'PAO 2024-1', N'Universidad Técnica de Ambato', N'Electricidad', 84.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-1') AS VARBINARY(MAX))),
        ('1805555555', '2024-09-01', N'PAO 2024-2', N'Universidad Técnica de Ambato', N'Electricidad', 88.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2024-2') AS VARBINARY(MAX))),
        ('1701111111', '2019-09-01', N'PAO 2019-2', N'Universidad Técnica de Ambato', N'Educación Básica', 82.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2019-2') AS VARBINARY(MAX))),
        ('1701111111', '2020-03-01', N'PAO 2020-1', N'Universidad Técnica de Ambato', N'Educación Básica', 85.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2020-1') AS VARBINARY(MAX))),
        ('1701111111', '2020-09-01', N'PAO 2020-2', N'Universidad Técnica de Ambato', N'Educación Básica', 88.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2020-2') AS VARBINARY(MAX))),
        ('1701111111', '2021-03-01', N'PAO 2021-1', N'Universidad Técnica de Ambato', N'Educación Básica', 90.00, 1, CAST(REPLACE(@pdf, '{T}', 'Evaluacion docente 2021-1') AS VARBINARY(MAX))),
        ('1701111111', '2025-03-01', N'PAO 2025-1', N'Universidad Técnica de Ambato', N'Gestión: Vicerrectorado Académico', 95.00, 0, CAST(REPLACE(@pdf, '{T}', 'Evaluacion de gestion 2025-1') AS VARBINARY(MAX)));
END

IF NOT EXISTS (SELECT 1 FROM dbo.CapacitacionImpartida)
BEGIN
    INSERT INTO dbo.CapacitacionImpartida (Cedula, NombreCurso, Horas) VALUES
        ('1801234567', N'Introducción a Python para docentes',      32),
        ('1712345678', N'Buenas prácticas de laboratorio',          40),
        ('1712345678', N'Escritura de artículos científicos',       24);
END

-- Equivalencias del CES art. 77: g) 16 h por comisión externa de concurso o par evaluador;
-- h) 24/32 h por facilitación externa del CES (una por promoción); i) 32 h por 2 meses en el CACES;
-- j) 3 evaluaciones académicas del CES equivalen a 1 dirección de trabajo de titulación.
IF NOT EXISTS (SELECT 1 FROM dbo.Equivalencias)
BEGIN
    INSERT INTO dbo.Equivalencias (Cedula, Tipo, Fecha, Meses, Descripcion, Pdf) VALUES
        ('1712345678', N'EvaluadorExternoConcurso',    DATEADD(MONTH, -10, @hoy), NULL, N'Miembro externo de comisión de concurso, UNACH', CAST(REPLACE(@pdf, '{T}', 'Certificado evaluador externo') AS VARBINARY(MAX))),
        ('1701111111', N'FacilitadorCESGradoMaestria', DATEADD(MONTH, -14, @hoy), NULL, N'Facilitadora externa CES, maestría en Educación', CAST(REPLACE(@pdf, '{T}', 'Certificado facilitadora CES') AS VARBINARY(MAX))),
        ('1701111111', N'EvaluacionAcademicaCES',      DATEADD(MONTH, -30, @hoy), NULL, N'Evaluación académica CES, carrera de Pedagogía', NULL),
        ('1701111111', N'EvaluacionAcademicaCES',      DATEADD(MONTH, -20, @hoy), NULL, N'Evaluación académica CES, maestría en Educación', NULL),
        ('1701111111', N'EvaluacionAcademicaCES',      DATEADD(MONTH, -12, @hoy), NULL, N'Evaluación académica CES, tecnología superior', NULL);
END
GO
