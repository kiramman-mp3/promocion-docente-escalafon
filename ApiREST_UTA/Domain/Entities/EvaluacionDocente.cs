namespace ApiREST_UTA.Domain.Entities
{
    public class EvaluacionDocente
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public DateTime Periodo { get; set; }
        public string Universidad { get; set; }
        public string Carrera { get; set; }
        public decimal Puntaje { get; set; }
        public byte[] Pdf { get; set; }
    }
}