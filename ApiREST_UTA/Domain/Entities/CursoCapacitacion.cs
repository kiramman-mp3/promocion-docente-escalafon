namespace ApiREST_UTA.Domain.Entities
{
    public class CursoCapacitacion
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Universidad { get; set; }
        public string Facultad { get; set; }
        public DateTime FechaEvaluacion { get; set; }
        public decimal Puntaje { get; set; }
    }
}
