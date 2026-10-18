namespace ApiREST_UTA.Application.DTOs
{
    public class RequisitoResponse
    {
        public string RolObjetivo { get; set; } = null!;
        public bool CumpleExperiencia { get; set; }
        public bool CumpleObras { get; set; }
        public bool CumpleEvaluacion { get; set; }
        public bool CumpleCapacitacion { get; set; }
        public bool CumpleInvestigacion { get; set; }
        public bool CumpleTesisDoctorado { get; set; }
        public string Observaciones { get; set; } = "";
    }
}
