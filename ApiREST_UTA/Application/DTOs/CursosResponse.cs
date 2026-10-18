namespace ApiREST_UTA.Application.DTOs
{
    public class CursosResponse
    {
        public string Cedula { get; set; }
        public string Universidad { get; set; }
        public string Facultad { get; set; }
        public DateTime FechaEvaluacion { get; set; }
        public decimal Puntaje { get; set; }
    }
}
