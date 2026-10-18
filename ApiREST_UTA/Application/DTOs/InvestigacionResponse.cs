namespace ApiREST_UTA.Application.DTOs
{
    public class InvestigacionResponse
    {
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Encargado { get; set; }
        public int Horas { get; set; }
        public string PdfBase64 { get; set; }
    }
}
