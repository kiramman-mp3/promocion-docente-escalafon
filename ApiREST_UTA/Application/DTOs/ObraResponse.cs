namespace ApiREST_UTA.Application.DTOs
{
    public class ObraResponse
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string NombreRevista { get; set; }
        public string PdfBase64 { get; set; }
    }
}
