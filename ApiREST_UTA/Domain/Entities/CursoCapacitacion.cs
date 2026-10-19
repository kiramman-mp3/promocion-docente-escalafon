namespace ApiREST_UTA.Domain.Entities
{
    // Curso de capacitación o actualización (DEaDV.dbo.CursosCapacitacion)
    public class CursoCapacitacion
    {
        public const string Pedagogica = "Pedagogica";
        public const string Cientifica = "Cientifica";
        public const string CongresoInternacional = "CongresoInternacional";

        public int Id { get; set; }
        public string Cedula { get; set; }
        public string? NombreCurso { get; set; }
        public string Universidad { get; set; }
        public string Facultad { get; set; }
        public DateTime FechaEvaluacion { get; set; }

        // Calificación obtenida en el curso (dato heredado, no se usa para el requisito)
        public decimal Puntaje { get; set; }

        public int Horas { get; set; }
        public string TipoCapacitacion { get; set; } = Cientifica;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // Fecha que cuenta para la ventana de 3 años del art. 76 d
        public DateTime FechaReferencia => FechaFin ?? FechaEvaluacion;

        // Art. 76 d: el 25 % debe ser actualización pedagógica o congresos internacionales
        public bool CuentaComoPedagogica => TipoCapacitacion is Pedagogica or CongresoInternacional;
    }
}
