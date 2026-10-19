namespace ApiREST_UTA.Domain.Entities
{
    // Producción académica (DIDE.dbo.Obras) con los datos que exigen los criterios de obra relevante del CES
    public class Obra
    {
        public const string ArticuloArbitrado = "ArticuloArbitrado";
        public const string Libro = "Libro";
        public const string CapituloLibro = "CapituloLibro";
        public const string Ponencia = "Ponencia";
        public const string PropiedadIndustrial = "PropiedadIndustrial";
        public const string ProduccionArtistica = "ProduccionArtistica";
        public const string DisenoSoftwarePrototipo = "DisenoSoftwarePrototipo";
        public const string Otra = "Otra";

        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public string? Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string NombreRevista { get; set; } = null!;
        public string Idioma { get; set; } = "Español";
        public bool FiliacionUTA { get; set; }
        public string TipoObra { get; set; } = ArticuloArbitrado;
        public string? ISSN { get; set; }
        public string? ISBN { get; set; }
        public string? BaseIndexacion { get; set; }
        public bool RevisionPares { get; set; }
        public string? RegistroSENADI { get; set; }
        public bool ValoracionExterna { get; set; }

        // Devuelve si la obra es "de relevancia" y, si no, qué criterio le falta (CES arts. 79-87)
        public (bool Relevante, string? Motivo) EvaluarRelevancia()
        {
            bool Tiene(string? valor) => !string.IsNullOrWhiteSpace(valor);

            return TipoObra switch
            {
                ArticuloArbitrado when !RevisionPares => (false, "sin revisión por pares (art. 82 a)"),
                ArticuloArbitrado when !Tiene(ISSN) => (false, "sin ISSN (art. 82 b)"),
                ArticuloArbitrado when !Tiene(BaseIndexacion) => (false, "la revista no está en un índice científico (art. 82 b)"),
                Libro or CapituloLibro when !RevisionPares => (false, "sin revisión por pares externos (arts. 80-81 a)"),
                Libro or CapituloLibro when !Tiene(ISBN) && !Tiene(ISSN) => (false, "sin ISBN ni ISSN (arts. 80-81 b)"),
                Ponencia when !Tiene(ISBN) && !Tiene(BaseIndexacion) => (false, "sin ISBN ni memoria indexada (art. 83 a)"),
                Ponencia when !RevisionPares => (false, "sin revisión por pares o comité científico (art. 83 b)"),
                PropiedadIndustrial when !Tiene(RegistroSENADI) => (false, "sin registro en el SENADI (art. 84)"),
                ProduccionArtistica or DisenoSoftwarePrototipo when !Tiene(RegistroSENADI) && !ValoracionExterna =>
                    (false, "sin registro en el SENADI ni valoración externa (arts. 85-86)"),
                Otra => (false, "tipo sin procedimiento de valoración aprobado por la universidad (art. 87)"),
                _ => (true, null)
            };
        }
    }
}
