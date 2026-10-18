namespace ApiREST_UTA.Domain.Entities
{
    // Recurso de apelación contra la resolución de la Comisión Académica (COA arts. 217-218 y 224)
    public class Apelacion
    {
        public const string Pendiente = "Pendiente";
        public const string Aceptada = "Aceptada";
        public const string Negada = "Negada";

        // COA art. 224: el recurso de apelación se interpone en el término de diez días
        public const int DiasTermino = 10;

        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public string Fundamentos { get; set; } = null!;
        public DateTime FechaPresentacion { get; set; }
        public int PresentadaPorId { get; set; }
        public string Estado { get; set; } = Pendiente;
        public string? ResolucionHCU { get; set; }
        public string? Motivacion { get; set; }
        public int? ResueltaPorId { get; set; }
        public DateTime? FechaResolucion { get; set; }

        // Último día para apelar: diez días hábiles (sin sábados ni domingos) desde la notificación de la resolución.
        // Los feriados no están modelados: Talento Humano debe considerarlos al resolver casos límite.
        public static DateTime PlazoHasta(DateTime fechaNotificacion)
        {
            var fecha = fechaNotificacion.Date;
            var habiles = 0;
            while (habiles < DiasTermino)
            {
                fecha = fecha.AddDays(1);
                if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                    habiles++;
            }
            return fecha;
        }
    }
}
