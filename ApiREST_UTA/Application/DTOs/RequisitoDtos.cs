namespace ApiREST_UTA.Application.DTOs
{
    public class RequisitoPromocionRequest
    {
        public int GradoOrigen { get; set; }
        public string Normativa { get; set; } = string.Empty;
        public string Resolucion { get; set; } = string.Empty;
        public bool EsPropuesta { get; set; }
        public DateTime VigenteDesde { get; set; }
        public int ObrasTotales { get; set; }
        public int ObrasEnGrado { get; set; } = 1;
        public decimal EvaluacionMinima { get; set; } = 75;
        public int EvaluacionPeriodos { get; set; } = 4;
        public int CapacitacionHoras { get; set; } = 96;
        public int CapacitacionAnios { get; set; } = 3;
        public decimal CapacitacionPctPedagogica { get; set; } = 25;
        public int ProyectosMeses { get; set; }
        public bool RequiereDireccionProyecto { get; set; }
        public int TrabajosTitulacion { get; set; }
        public string TitulacionNivelMinimo { get; set; } = "Grado";
    }

    public class RequisitoPromocionResponse
    {
        public int Id { get; set; }
        public int GradoOrigen { get; set; }
        public string RolOrigen { get; set; } = null!;
        public int GradoDestino { get; set; }
        public string RolDestino { get; set; } = null!;
        public string Normativa { get; set; } = null!;
        public string Resolucion { get; set; } = null!;
        public bool EsPropuesta { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime? VigenteHasta { get; set; }
        public bool Vigente { get; set; }
        public int ObrasTotales { get; set; }
        public int ObrasEnGrado { get; set; }
        public decimal EvaluacionMinima { get; set; }
        public int EvaluacionPeriodos { get; set; }
        public int CapacitacionHoras { get; set; }
        public int CapacitacionAnios { get; set; }
        public decimal CapacitacionPctPedagogica { get; set; }
        public int ProyectosMeses { get; set; }
        public bool RequiereDireccionProyecto { get; set; }
        public int TrabajosTitulacion { get; set; }
        public string TitulacionNivelMinimo { get; set; } = null!;
    }
}
