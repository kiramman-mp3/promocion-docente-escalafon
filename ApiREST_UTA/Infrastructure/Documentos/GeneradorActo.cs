using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using System.Globalization;

namespace ApiREST_UTA.Infrastructure.Documentos
{
    // Resolución de la Comisión Académica sobre una solicitud de promoción (CES art. 77 l; UTA art. 47)
    public class GeneradorActo : IGeneradorActo
    {
        private static readonly CultureInfo Es = new("es-EC");
        private static readonly TimeZoneInfo ZonaEcuador = BuscarZona();

        public byte[] Generar(DatosActo datos)
        {
            var s = datos.Solicitud;
            var aprobada = s.Estado is SolicitudPromocion.Aprobada or SolicitudPromocion.Ejecutada;
            var fecha = ALocal(s.FechaResolucion ?? DateTime.UtcNow);
            var pdf = new DocumentoPdf();

            pdf.Texto("UNIVERSIDAD TÉCNICA DE AMBATO", 14, negrita: true, espacioDespues: 0);
            pdf.Texto("Comisión Académica de Promoción del Personal Académico", 11, espacioDespues: 8);
            pdf.Texto($"RESOLUCIÓN N.º {s.NumeroResolucion}", 13, negrita: true, espacioDespues: 0);
            pdf.Texto($"Ambato, {fecha.ToString("d 'de' MMMM 'de' yyyy", Es)}", 10, espacioDespues: 6);
            pdf.Linea();

            pdf.Texto("CONSIDERANDO:", 11, negrita: true);
            var considerandos = new List<string>
            {
                "Que, el artículo 47 del Reglamento de Carrera y Escalafón del Personal Académico de la Universidad Técnica de Ambato " +
                "(Resolución 0153-HCU-2026) establece que la Comisión Académica es responsable de los procesos de promoción del personal académico titular;",
                "Que, el artículo 76 del Reglamento de Carrera y Escalafón del Personal Académico del Sistema de Educación Superior " +
                "(RPC-SE-19-No.055-2021) y el artículo 48 del reglamento institucional fijan los requisitos para la promoción al grado escalafonario siguiente;",
                $"Que, el {ALocal(s.FechaSolicitud).ToString("d 'de' MMMM 'de' yyyy", Es)} {datos.NombreDocente}, con cédula {s.Cedula}, " +
                $"solicitó su promoción de {s.RolActual} a {s.RolObjetivo}, conforme al artículo 99 del reglamento del CES;",
                s.FechaEnvioComision.HasValue
                    ? $"Que, la Dirección de Talento Humano revisó la documentación del expediente y la remitió a esta Comisión el " +
                      $"{ALocal(s.FechaEnvioComision.Value).ToString("d 'de' MMMM 'de' yyyy", Es)};"
                    : "Que, la Dirección de Talento Humano revisó la documentación del expediente;",
                "Que, el artículo 77 literal l) del reglamento del CES dispone que la promoción conste en una acción de personal " +
                "o en el acto administrativo respectivo, con la fecha en que se llevó a cabo;"
            };
            if (datos.Dictamen?.Normativa != null)
                considerandos.Add($"Que, los requisitos aplicados son los vigentes al presentar la solicitud: {datos.Dictamen.Normativa}" +
                    $"{(datos.Dictamen.RequisitosPropuestos ? " (propuesta institucional pendiente de la normativa interna de promoción)" : "")};");
            foreach (var c in considerandos)
                pdf.Texto(c, 10, sangria: 12);

            if (datos.Dictamen != null)
            {
                pdf.Espacio(4);
                pdf.Texto("Verificación de requisitos", 11, negrita: true);
                foreach (var r in datos.Dictamen.Requisitos)
                {
                    var estado = r.Cumple ? (r.Exento ? "Cumple (con excepción o equivalencia)" : "Cumple") : "No cumple";
                    pdf.Texto($"{r.Nombre} ({r.Articulo}): {estado}.", 10, negrita: true, sangria: 12, espacioDespues: 0);
                    pdf.Texto($"Obtenido: {r.Obtenido}. Requerido: {r.Requerido}. {r.Detalle}".Trim(), 9, sangria: 24, espacioDespues: 4);
                }
            }

            if (datos.Votos.Count > 0)
            {
                pdf.Espacio(4);
                pdf.Texto("Votación de la Comisión", 11, negrita: true);
                foreach (var v in datos.Votos)
                {
                    var voto = v.Voto switch
                    {
                        VotoComision.AFavor => "a favor",
                        VotoComision.EnContra => "en contra",
                        _ => "abstención"
                    };
                    var presupuesto = v.CertificaPresupuesto == true ? " Certifica la disponibilidad presupuestaria." : "";
                    pdf.Texto($"{v.Miembro} ({Cargo(v.Cargo)}{(v.EsPresidente ? ", preside" : "")}): {voto}. {v.Motivacion}{presupuesto}",
                        9, sangria: 12, espacioDespues: 3);
                }
            }

            if (datos.ResolucionApelacion != null)
            {
                pdf.Espacio(4);
                pdf.Texto("Apelación", 11, negrita: true);
                pdf.Texto($"El Honorable Consejo Universitario aceptó la apelación mediante resolución {datos.ResolucionApelacion}. {datos.MotivacionApelacion}",
                    10, sangria: 12);
            }

            pdf.Espacio(4);
            pdf.Texto("RESUELVE:", 11, negrita: true);
            pdf.Texto($"Artículo 1.- {(aprobada ? "APROBAR" : "NEGAR")} la promoción de {datos.NombreDocente}, con cédula {s.Cedula}, " +
                      $"de {s.RolActual} a {s.RolObjetivo}.", 10, sangria: 12);
            pdf.Texto(aprobada
                    ? "Artículo 2.- Disponer a la Dirección de Talento Humano que emita la acción de personal correspondiente " +
                      "(artículo 77 literal l del reglamento del CES). La promoción no cambia el tiempo de dedicación (artículo 76)."
                    : "Artículo 2.- Notificar a la persona solicitante, que podrá interponer recurso de apelación en el término de diez días " +
                      "(Código Orgánico Administrativo, artículos 217 y 224).",
                10, sangria: 12);
            if (!string.IsNullOrWhiteSpace(s.Observacion))
                pdf.Texto($"Motivación: {s.Observacion}", 10, sangria: 12);

            pdf.Firma(datos.Presidente ?? "Presidencia de la Comisión", "Presidente de la Comisión Académica de Promoción");
            pdf.Firma("Secretaría", "Secretario(a) de la Comisión Académica de Promoción");

            return pdf.Generar($"Resolución {s.NumeroResolucion} | Solicitud {s.Id}");
        }

        private static string Cargo(string? cargo) => cargo switch
        {
            MiembroComision.VicerrectorAcademico => "Vicerrector Académico",
            MiembroComision.ProfesorHCU => "profesor designado por el HCU",
            MiembroComision.ProfesorTerna => "profesor de terna",
            MiembroComision.DirectorFinanciero => "Director Financiero",
            MiembroComision.DirectorTalentoHumano => "Director de Talento Humano",
            _ => cargo ?? "miembro"
        };

        private static DateTime ALocal(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ZonaEcuador);

        private static TimeZoneInfo BuscarZona()
        {
            foreach (var id in new[] { "America/Guayaquil", "SA Pacific Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
            }
            return TimeZoneInfo.CreateCustomTimeZone("ECT", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");
        }
    }
}
