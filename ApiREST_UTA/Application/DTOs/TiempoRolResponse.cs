namespace ApiREST_UTA.Application.DTOs
{
    public class TiempoRolResponse
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string RolActual { get; set; }
        public int AniosEnRol { get; set; }
        public DateTime FechaInicio { get; set; }
        public string PdfBase64 { get; set; }
    }
}
