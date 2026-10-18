namespace ApiREST_UTA.Domain.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int RequiereAniosEnRol { get; set; }
        public int RequiereObras { get; set; }
        public decimal RequiereEvaluacion { get; set; }
        public int RequiereCapacitacionHoras { get; set; }
        public int RequiereInvestigacionMeses { get; set; }
    }
}
