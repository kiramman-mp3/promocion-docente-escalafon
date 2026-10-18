using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services.Promocion;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace ApiREST_UTA.Application.Services
{
    // Flujo de la solicitud de promoción:
    // 1. El docente la presenta con su dictamen (CES art. 99).
    // 2. Talento Humano revisa cada documento del expediente y la envía a la Comisión Académica.
    // 3. La Comisión vota (UTA art. 47) y su presidente cierra la votación: Aprobada o Rechazada.
    public class SolicitudService
    {
        // UTA art. 47 no fija quórum; decisión D3 del plan: 4 de los 6 miembros
        public const int Quorum = 4;

        private static readonly JsonSerializerOptions JsonWeb = new(JsonSerializerDefaults.Web);

        private readonly ISolicitudRepository _solicitudRepo;
        private readonly PromocionService _promocion;
        private readonly IComisionRepository _comision;
        private readonly IInvestigacionRepository _investigaciones;
        private readonly IEvidenciaRepository _evidencias;

        public SolicitudService(
            ISolicitudRepository solicitudRepo,
            PromocionService promocion,
            IComisionRepository comision,
            IInvestigacionRepository investigaciones,
            IEvidenciaRepository evidencias)
        {
            _solicitudRepo = solicitudRepo;
            _promocion = promocion;
            _comision = comision;
            _investigaciones = investigaciones;
            _evidencias = evidencias;
        }

        public async Task<SolicitudResponse> CrearAsync(string cedula, ClaimsPrincipal usuario)
        {
            cedula = cedula.Trim();
            var dictamen = await _promocion.EvaluarAsync(cedula);

            // CES art. 76: Agregado 3 y Principal 3 no tienen promoción
            if (!dictamen.Promocionable)
                throw ReglaNegocioException.Invalido(dictamen.Requisito(ReglaGradoPromocionable.Codigo)!.Detalle);
            if (dictamen.RequisitosId == null)
                throw ReglaNegocioException.Conflicto($"No hay requisitos vigentes para promover desde {dictamen.RolActual}.");

            // CES art. 99: se solicita después de cumplir la permanencia mínima en el grado
            var permanencia = dictamen.Requisito(ReglaPermanencia.Codigo)!;
            if (!permanencia.Cumple)
                throw ReglaNegocioException.Invalido(
                    $"Todavía no cumple la permanencia mínima para solicitar la promoción: tiene {permanencia.Obtenido} y se exigen {permanencia.Requerido} (CES art. 99).");

            if (await _solicitudRepo.ExisteEnTramiteAsync(cedula))
                throw ReglaNegocioException.Conflicto("Ya hay una solicitud en trámite para esta cédula.");

            // RRHH es una base externa: tras aprobar, el rol actual no cambia hasta que RRHH lo actualice
            var aprobadas = (await _solicitudRepo.ListarAsync(null, cedula))
                .Where(s => s.Estado is SolicitudPromocion.Aprobada or SolicitudPromocion.Ejecutada);
            if (aprobadas.Any(s => s.RolObjetivo == dictamen.RolObjetivo))
                throw ReglaNegocioException.Conflicto(
                    $"La promoción a {dictamen.RolObjetivo} ya fue aprobada; falta que RRHH registre el nuevo rol.");

            var id = await _solicitudRepo.CrearAsync(new SolicitudPromocion
            {
                Cedula = cedula,
                RolActualId = dictamen.RolActualId,
                RolActual = dictamen.RolActual,
                RolObjetivo = dictamen.RolObjetivo!,
                SolicitadoPorId = usuario.Id(),
                RequisitosPromocionId = dictamen.RequisitosId,
                DictamenJson = JsonSerializer.Serialize(dictamen, JsonWeb)
            });

            return (await ObtenerAsync(id, usuario))!;
        }

        public async Task<List<SolicitudResponse>> ListarAsync(string? estado, ClaimsPrincipal usuario)
        {
            estado = string.IsNullOrWhiteSpace(estado) ? null : estado;
            List<SolicitudPromocion> solicitudes;
            if (usuario.EsRevisor())
            {
                solicitudes = await _solicitudRepo.ListarAsync(estado, null);
            }
            else
            {
                // Las propias y, para los miembros de la Comisión, las que ya llegaron a ella
                solicitudes = string.IsNullOrEmpty(usuario.CedulaPropia())
                    ? new List<SolicitudPromocion>()
                    : await _solicitudRepo.ListarAsync(estado, usuario.CedulaPropia());
                if (usuario.EsMiembroComision())
                {
                    var deComision = await _solicitudRepo.ListarAsync(estado, null, soloEnviadasAComision: true);
                    solicitudes = solicitudes.Concat(deComision).GroupBy(s => s.Id).Select(g => g.First())
                        .OrderBy(s => s.EnTramite ? 0 : 1).ThenByDescending(s => s.FechaSolicitud).ToList();
                }
            }
            return solicitudes.Select(s => AResponse(s)).ToList();
        }

        public async Task<SolicitudResponse?> ObtenerAsync(int id, ClaimsPrincipal usuario)
        {
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id);
            if (solicitud == null) return null;
            if (!usuario.PuedeVerSolicitud(solicitud))
                throw ReglaNegocioException.Prohibido("Solo puedes ver tus propias solicitudes.");

            var revisiones = await _solicitudRepo.ObtenerRevisionesAsync(id);
            var response = AResponse(solicitud);
            response.Revisiones = revisiones.Select(AResponse).ToList();
            response.Dictamen = string.IsNullOrEmpty(solicitud.DictamenJson)
                ? await _promocion.EvaluarAsync(solicitud.Cedula, solicitud.RequisitosPromocionId)
                : JsonSerializer.Deserialize<DictamenPromocion>(solicitud.DictamenJson, JsonWeb);
            response.Documentos = await DocumentosAsync(solicitud, revisiones);

            var votos = await _comision.ObtenerVotosAsync(id);
            var miembro = await _comision.ObtenerMiembroActivoPorUsuarioAsync(usuario.Id());
            var resuelta = solicitud.Estado is not SolicitudPromocion.RevisionDocumental and not SolicitudPromocion.EnComision;
            // Los votos son visibles para la Comisión y Talento Humano durante la votación, y para todos al resolverse
            if (resuelta || miembro != null || usuario.EsRevisor())
                response.Votos = votos.Select(AResponse).ToList();
            if (solicitud.LlegoAComision)
                response.Votacion = await VotacionAsync(solicitud, votos, miembro);

            return response;
        }

        public async Task<SolicitudResponse> RevisarDocumentoAsync(int id, RevisarDocumentoRequest request, ClaimsPrincipal usuario)
        {
            var solicitud = await ObtenerEnEstadoAsync(id, SolicitudPromocion.RevisionDocumental);

            if (!RevisionDocumento.Tipos.Contains(request.TipoDocumento))
                throw ReglaNegocioException.Invalido($"Tipo de documento inválido. Usa: {string.Join(", ", RevisionDocumento.Tipos)}.");
            if (request.Estado != RevisionDocumento.Aprobado && request.Estado != RevisionDocumento.Rechazado)
                throw ReglaNegocioException.Invalido("El estado debe ser Aprobado o Rechazado.");
            if (request.Estado == RevisionDocumento.Rechazado && string.IsNullOrWhiteSpace(request.Observacion))
                throw ReglaNegocioException.Invalido("Indica el motivo del rechazo del documento.");

            // El documento debe pertenecer al expediente de esta solicitud
            var documentos = await DocumentosAsync(solicitud, new List<RevisionDocumento>());
            if (!documentos.Any(d => d.Tipo == request.TipoDocumento && d.Id == request.DocumentoId))
                throw ReglaNegocioException.Invalido("El documento no pertenece al expediente de esta solicitud.");

            await _solicitudRepo.GuardarRevisionAsync(new RevisionDocumento
            {
                SolicitudId = id,
                TipoDocumento = request.TipoDocumento,
                DocumentoId = request.DocumentoId,
                Estado = request.Estado,
                Observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim(),
                RevisadoPorId = usuario.Id()
            });

            return (await ObtenerAsync(id, usuario))!;
        }

        // Talento Humano envía el expediente revisado a la Comisión Académica (UTA art. 47)
        public async Task<SolicitudResponse> EnviarAComisionAsync(int id, ClaimsPrincipal usuario)
        {
            var solicitud = await ObtenerEnEstadoAsync(id, SolicitudPromocion.RevisionDocumental);

            var revisiones = await _solicitudRepo.ObtenerRevisionesAsync(id);
            var pendientes = (await DocumentosAsync(solicitud, revisiones)).Count(d => d.Revision == null);
            if (pendientes > 0)
                throw ReglaNegocioException.Conflicto(
                    $"Faltan {(pendientes == 1 ? "1 documento" : $"{pendientes} documentos")} por revisar antes de enviar a la Comisión.");

            // Dictamen definitivo con los requisitos vigentes al presentar la solicitud (CES, transitoria novena)
            var dictamen = await _promocion.EvaluarAsync(solicitud.Cedula, solicitud.RequisitosPromocionId);
            if (!await _solicitudRepo.EnviarAComisionAsync(id, usuario.Id(), JsonSerializer.Serialize(dictamen, JsonWeb)))
                throw ReglaNegocioException.Conflicto("La solicitud ya fue enviada a la Comisión.");

            return (await ObtenerAsync(id, usuario))!;
        }

        public async Task<SolicitudResponse> VotarAsync(int id, VotoRequest request, ClaimsPrincipal usuario)
        {
            var solicitud = await ObtenerEnEstadoAsync(id, SolicitudPromocion.EnComision);
            var miembro = await _comision.ObtenerMiembroActivoPorUsuarioAsync(usuario.Id())
                ?? throw ReglaNegocioException.Prohibido("Solo los miembros vigentes de la Comisión Académica pueden votar.");

            if (miembro.Cedula != null && miembro.Cedula == solicitud.Cedula)
                throw ReglaNegocioException.Prohibido("No puedes votar tu propia solicitud de promoción (conflicto de interés).");
            if (!VotoComision.Opciones.Contains(request.Voto))
                throw ReglaNegocioException.Invalido("El voto debe ser AFavor, EnContra o Abstencion.");
            if (string.IsNullOrWhiteSpace(request.Motivacion))
                throw ReglaNegocioException.Invalido("Motiva tu voto.");
            if (request.CertificaPresupuesto.HasValue && miembro.Cargo != MiembroComision.DirectorFinanciero)
                throw ReglaNegocioException.Invalido("Solo el Director Financiero certifica la disponibilidad presupuestaria.");

            await _comision.GuardarVotoAsync(new VotoComision
            {
                SolicitudId = id,
                MiembroId = miembro.Id,
                Voto = request.Voto,
                Motivacion = request.Motivacion.Trim(),
                CertificaPresupuesto = miembro.Cargo == MiembroComision.DirectorFinanciero ? request.CertificaPresupuesto ?? false : null
            });

            return (await ObtenerAsync(id, usuario))!;
        }

        // El presidente cierra la votación: mayoría simple con quórum; en empate decide su voto (UTA art. 47)
        public async Task<SolicitudResponse> CerrarVotacionAsync(int id, CerrarVotacionRequest request, ClaimsPrincipal usuario)
        {
            var solicitud = await ObtenerEnEstadoAsync(id, SolicitudPromocion.EnComision);
            var miembro = await _comision.ObtenerMiembroActivoPorUsuarioAsync(usuario.Id());
            if (miembro == null || !miembro.EsPresidente)
                throw ReglaNegocioException.Prohibido("Solo quien preside la Comisión Académica puede cerrar la votación.");
            if (string.IsNullOrWhiteSpace(request.Motivacion))
                throw ReglaNegocioException.Invalido("Escribe la motivación de la resolución (Código Orgánico Administrativo, art. 100).");

            var votos = await _comision.ObtenerVotosAsync(id);
            var (estado, explicacion) = ResultadoVotacion(votos);

            await ResolverAsync(solicitud, SolicitudPromocion.EnComision, estado, $"{request.Motivacion.Trim()} {explicacion}", usuario);
            return (await ObtenerAsync(id, usuario))!;
        }

        // Regla de decisión de la Comisión. Devuelve el estado resultante y una explicación para el acta.
        public static (string Estado, string Explicacion) ResultadoVotacion(List<VotoComision> votos)
        {
            if (votos.Count < Quorum)
                throw ReglaNegocioException.Conflicto($"No hay quórum: votaron {votos.Count} de los {Quorum} miembros necesarios.");

            var aFavor = votos.Count(v => v.Voto == VotoComision.AFavor);
            var enContra = votos.Count(v => v.Voto == VotoComision.EnContra);
            var presidente = votos.FirstOrDefault(v => v.EsPresidente);

            bool aprueba;
            var conteo = $"Votación: {aFavor} a favor, {enContra} en contra, {votos.Count - aFavor - enContra} abstenciones.";
            if (aFavor != enContra)
            {
                aprueba = aFavor > enContra;
            }
            else
            {
                // Empate: voto dirimente del presidente; si no votó a favor, no se aprueba
                aprueba = presidente?.Voto == VotoComision.AFavor;
                conteo += " Empate resuelto con el voto dirimente de la presidencia.";
            }

            if (aprueba && !votos.Any(v => v.Cargo == MiembroComision.DirectorFinanciero && v.CertificaPresupuesto == true))
                throw ReglaNegocioException.Conflicto(
                    "No se puede aprobar sin la certificación de disponibilidad presupuestaria del Director Financiero (CES art. 99).");

            return (aprueba ? SolicitudPromocion.Aprobada : SolicitudPromocion.Rechazada, conteo);
        }

        private async Task ResolverAsync(SolicitudPromocion solicitud, string estadoEsperado, string nuevoEstado, string motivacion, ClaimsPrincipal usuario)
        {
            var numero = solicitud.NumeroResolucion ?? $"CAP-{DateTime.UtcNow:yyyy}-{solicitud.Id:D4}";
            if (!await _solicitudRepo.ResolverAsync(solicitud.Id, estadoEsperado, nuevoEstado, numero, motivacion, usuario.Id()))
                throw ReglaNegocioException.Conflicto("La solicitud cambió de estado mientras se resolvía.");
        }

        private async Task<SolicitudPromocion> ObtenerEnEstadoAsync(int id, string estado)
        {
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id)
                ?? throw ReglaNegocioException.NoEncontrado("La solicitud no existe.");
            if (solicitud.Estado != estado)
                throw ReglaNegocioException.Conflicto($"Esta acción requiere la solicitud en estado {Nombre(estado)} y está {Nombre(solicitud.Estado)}.");
            return solicitud;
        }

        private static string Nombre(string estado) => estado switch
        {
            SolicitudPromocion.RevisionDocumental => "revisión documental",
            SolicitudPromocion.EnComision => "en comisión",
            SolicitudPromocion.Aprobada => "aprobada",
            SolicitudPromocion.Rechazada => "rechazada",
            SolicitudPromocion.Apelada => "apelada",
            SolicitudPromocion.Ejecutada => "ejecutada",
            _ => estado
        };

        private async Task<VotacionResponse> VotacionAsync(SolicitudPromocion solicitud, List<VotoComision> votos, MiembroComision? miembro)
        {
            var activos = await _comision.ListarMiembrosAsync(incluirHistorico: false);
            var miVoto = miembro == null ? null : votos.FirstOrDefault(v => v.MiembroId == miembro.Id);
            var enComision = solicitud.Estado == SolicitudPromocion.EnComision;

            string? motivo = null;
            if (miembro == null) motivo = "No integras la Comisión Académica.";
            else if (!enComision) motivo = "La votación está cerrada.";
            else if (miembro.Cedula != null && miembro.Cedula == solicitud.Cedula) motivo = "Es tu propia solicitud (conflicto de interés).";

            return new VotacionResponse
            {
                MiembrosActivos = activos.Count,
                Quorum = Quorum,
                VotosEmitidos = votos.Count,
                AFavor = votos.Count(v => v.Voto == VotoComision.AFavor),
                EnContra = votos.Count(v => v.Voto == VotoComision.EnContra),
                Abstenciones = votos.Count(v => v.Voto == VotoComision.Abstencion),
                PresupuestoCertificado = votos.Any(v => v.Cargo == MiembroComision.DirectorFinanciero && v.CertificaPresupuesto == true),
                EsMiembro = miembro != null,
                EsPresidente = miembro?.EsPresidente ?? false,
                MiCargo = miembro?.Cargo,
                PuedeVotar = motivo == null,
                MotivoNoPuedeVotar = motivo,
                PuedeCerrar = enComision && (miembro?.EsPresidente ?? false),
                MiVoto = miVoto == null ? null : AResponse(miVoto)
            };
        }

        // Documentos del expediente que Talento Humano debe revisar
        private async Task<List<DocumentoSolicitudResponse>> DocumentosAsync(SolicitudPromocion solicitud, List<RevisionDocumento> revisiones)
        {
            var c = await _promocion.ConstruirContextoAsync(solicitud.Cedula, solicitud.RequisitosPromocionId, DateTime.Today);
            var investigaciones = await _investigaciones.ObtenerInvestigacionesPorCedula(solicitud.Cedula);
            var evidencias = await _evidencias.ListarAsync(solicitud.Id);

            var documentos = new List<DocumentoSolicitudResponse>
            {
                new() { Tipo = "TiempoRol", Id = c.TiempoRolId, Descripcion = $"Acción de personal: {c.RolActual.Nombre} desde el {c.FechaInicioGrado:dd/MM/yyyy}" }
            };
            documentos.AddRange(c.Obras.OrderByDescending(o => o.FechaPublicacion).Select(o => new DocumentoSolicitudResponse
            {
                Tipo = "Obra",
                Id = o.Id,
                Descripcion = $"{o.Titulo ?? o.NombreRevista} ({o.NombreRevista}, {o.FechaPublicacion:yyyy})"
                    + (o.EvaluarRelevancia() is { Relevante: false } r ? $": no relevante, {r.Motivo}" : "")
            }));
            documentos.AddRange(c.Evaluaciones.OrderByDescending(e => e.Periodo).Select(e => new DocumentoSolicitudResponse
            {
                Tipo = "Evaluacion",
                Id = e.Id,
                Descripcion = $"Evaluación {e.PeriodoAcademico ?? e.Periodo.ToString("MM/yyyy")}: {e.Puntaje:0.##} %" + (e.EjercioDocencia ? "" : " (sin docencia)")
            }));
            documentos.AddRange(c.Cursos.Select(x => new DocumentoSolicitudResponse
            {
                Tipo = "Curso",
                Id = x.Id,
                Descripcion = $"{x.NombreCurso ?? x.Universidad}: {x.Horas} h, {x.TipoCapacitacion} ({x.FechaReferencia:MM/yyyy})"
            }));
            documentos.AddRange(investigaciones.Select(i => new DocumentoSolicitudResponse
            {
                Tipo = "Investigacion",
                Id = i.Id,
                Descripcion = $"{i.Nombre}: {i.Horas} h"
            }));
            documentos.AddRange(c.Proyectos.Select(p => new DocumentoSolicitudResponse
            {
                Tipo = "Proyecto",
                Id = p.Id,
                Descripcion = $"{p.Rol} de {p.NombreProyecto} ({p.TipoProyecto}, {p.Meses} meses)"
            }));
            documentos.AddRange(c.TrabajosTitulacion.Select(t => new DocumentoSolicitudResponse
            {
                Tipo = "TrabajoTitulacion",
                Id = t.Id,
                Descripcion = $"{t.Rol} de tesis de {t.Nivel.ToLower()}: {t.Titulo}"
            }));
            documentos.AddRange(c.Equivalencias.Select(e => new DocumentoSolicitudResponse
            {
                Tipo = "Equivalencia",
                Id = e.Id,
                Descripcion = $"{e.Descripcion} ({e.Tipo})"
            }));
            documentos.AddRange(evidencias.Select(e => new DocumentoSolicitudResponse
            {
                Tipo = "Evidencia",
                Id = e.Id,
                Descripcion = $"Evidencia del docente: {e.Descripcion} ({e.NombreArchivo})"
            }));

            foreach (var d in documentos)
            {
                var revision = revisiones.FirstOrDefault(r => r.TipoDocumento == d.Tipo && r.DocumentoId == d.Id);
                d.Revision = revision == null ? null : AResponse(revision);
            }
            return documentos;
        }

        private static SolicitudResponse AResponse(SolicitudPromocion s) => new()
        {
            Id = s.Id,
            Cedula = s.Cedula,
            RolActualId = s.RolActualId,
            RolActual = s.RolActual,
            RolObjetivo = s.RolObjetivo,
            Estado = s.Estado,
            SolicitadoPor = s.SolicitadoPor,
            FechaSolicitud = s.FechaSolicitud,
            RequisitosPromocionId = s.RequisitosPromocionId,
            FechaEnvioComision = s.FechaEnvioComision,
            NumeroResolucion = s.NumeroResolucion,
            ResueltoPor = s.ResueltoPor,
            FechaResolucion = s.FechaResolucion,
            Observacion = s.Observacion
        };

        private static RevisionDocumentoResponse AResponse(RevisionDocumento r) => new()
        {
            TipoDocumento = r.TipoDocumento,
            DocumentoId = r.DocumentoId,
            Estado = r.Estado,
            Observacion = r.Observacion,
            RevisadoPor = r.RevisadoPor,
            FechaRevision = r.FechaRevision
        };

        private static VotoResponse AResponse(VotoComision v) => new()
        {
            Miembro = v.Miembro,
            Cargo = v.Cargo,
            EsPresidente = v.EsPresidente,
            Voto = v.Voto,
            Motivacion = v.Motivacion,
            CertificaPresupuesto = v.CertificaPresupuesto,
            Fecha = v.Fecha
        };
    }
}
