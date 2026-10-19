namespace ApiREST_UTA.Application.DTOs
{
    public class ObraResponse
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string? Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string NombreRevista { get; set; }
        public string Idioma { get; set; } = "Español";
        public string TipoObra { get; set; } = "ArticuloArbitrado";
        public string? ISSN { get; set; }
        public string? ISBN { get; set; }
        public string? BaseIndexacion { get; set; }
        public bool RevisionPares { get; set; }
        public string? RegistroSENADI { get; set; }

        // Resultado de los criterios de obra relevante (CES arts. 79-87)
        public bool EsRelevante { get; set; }
        public string? MotivoNoRelevante { get; set; }
        public string PdfBase64 { get; set; }
    }
}
