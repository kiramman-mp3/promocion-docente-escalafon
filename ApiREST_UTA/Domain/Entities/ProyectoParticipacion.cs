namespace ApiREST_UTA.Domain.Entities
{
    // Participación en proyectos de investigación o vinculación (DIDE.dbo.ParticipacionProyecto)
    public class ProyectoParticipacion
    {
        public const string Director = "Director";
        public const string Codirector = "Codirector";
        public const string Participante = "Participante";
        public const string Investigacion = "Investigacion";
        public const string Vinculacion = "Vinculacion";

        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public string? NombreProyecto { get; set; }
        public string Rol { get; set; } = Participante;
        public string TipoProyecto { get; set; } = Investigacion;
        public int Meses { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // CES art. 36 c.2: la dirección cuenta el doble y la codirección 1,5 veces
        public decimal MesesPonderados => Rol switch
        {
            Director => Meses * 2m,
            Codirector => Meses * 1.5m,
            _ => Meses
        };

        public bool EsDireccion => Rol is Director or Codirector;
    }
}
