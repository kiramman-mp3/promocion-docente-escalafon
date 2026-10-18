USE DAC;
GO

-- Trabajos de titulación dirigidos (UTA art. 48 e.3). Para Principal se exigen de maestría o más;
-- un doctorado equivale a 3 maestrías (CES art. 36 b.3).
IF NOT EXISTS (SELECT 1 FROM dbo.TrabajosTitulacionDirigidos)
BEGIN
    INSERT INTO dbo.TrabajosTitulacionDirigidos (Cedula, Titulo, Nivel, Rol, FechaAprobacion, Universidad) VALUES
        ('1801234567', N'Sistema de recomendación de cursos para estudiantes',            N'Grado',     N'Director',   '2022-08-15', N'Universidad Técnica de Ambato'),
        ('1801234567', N'Detección de plagio en tareas de programación',                  N'Grado',     N'Director',   '2024-02-20', N'Universidad Técnica de Ambato'),
        ('1712345678', N'Encapsulación de probióticos en matrices vegetales',             N'Doctorado', N'Director',   '2021-11-10', N'Universidad Técnica de Ambato'),
        ('1712345678', N'Valorización de residuos de mora de Castilla',                   N'Doctorado', N'Director',   '2023-06-05', N'Universidad de Cuenca'),
        ('1805555555', N'Eficiencia energética en edificios universitarios',              N'Maestria',  N'Director',   '2022-07-01', N'Universidad Técnica de Ambato'),
        ('1701111111', N'Competencias digitales en docentes rurales',                     N'Grado',     N'Director',   '2018-03-12', N'Universidad Técnica de Ambato'),
        ('1701111111', N'Lectura crítica en educación básica',                            N'Maestria',  N'Codirector', '2020-10-30', N'Universidad Técnica de Ambato');
END
GO
