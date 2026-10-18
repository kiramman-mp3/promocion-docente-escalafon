USE DAC;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TesisDoctorado)
BEGIN
    INSERT INTO dbo.TesisDoctorado (Cedula, Titulo, Universidad, EsDirigida) VALUES
        ('1712345678', N'Encapsulación de probióticos en matrices vegetales', N'Universidad Técnica de Ambato', 1),
        ('1712345678', N'Valorización de residuos de mora de Castilla',       N'Universidad de Cuenca',         1);
END
GO
