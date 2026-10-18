namespace ApiREST_UTA.Domain.Entities
{
    // Documento que el docente anexa a su solicitud (CES art. 99) y que no está en las bases institucionales
    public class EvidenciaSolicitud
    {
        public const int TamanoMaximo = 10 * 1024 * 1024;
        public static readonly string[] Tipos =
            { "CertificadoCongreso", "Capacitacion", "Equivalencia", "Sabatico", "CargoAutoridad", "Obra", "Proyecto", "Otro" };

        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public string Tipo { get; set; } = "Otro";
        public string Descripcion { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public byte[]? Contenido { get; set; }
        public string Hash { get; set; } = null!;
        public int Tamano { get; set; }
        public int SubidoPorId { get; set; }
        public string? SubidoPor { get; set; }
        public DateTime FechaSubida { get; set; }

        // Un PDF real empieza con "%PDF-"; no basta con la extensión ni el tipo que declara el navegador
        public static bool EsPdf(byte[] contenido) =>
            contenido.Length > 5 && contenido[0] == '%' && contenido[1] == 'P' && contenido[2] == 'D' && contenido[3] == 'F' && contenido[4] == '-';
    }
}
