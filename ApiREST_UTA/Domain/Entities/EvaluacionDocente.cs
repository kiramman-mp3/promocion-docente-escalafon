namespace ApiREST_UTA.Domain.Entities
{
    // Resultado de la evaluación integral de desempeño de un periodo (DEaDV.dbo.EvaluacionesDocentes)
    public class EvaluacionDocente
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public DateTime Periodo { get; set; }
        public string? PeriodoAcademico { get; set; }
        public string Universidad { get; set; }
        public string Carrera { get; set; }

        // Porcentaje de 0 a 100
        public decimal Puntaje { get; set; }

        // Art. 76 c: solo cuentan los procesos en los que ejerció la docencia
        public bool EjercioDocencia { get; set; } = true;
        public byte[] Pdf { get; set; }
    }
}
