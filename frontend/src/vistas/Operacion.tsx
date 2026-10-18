import { useEffect, useState, type FormEvent } from "react";
import { consultarAuditoria, estadoSalud, reporteUso } from "../api";
import { formatoFechaHora, formatoNumero, nombreEstado } from "../format";
import { useSesion } from "../sesion";
import type { EstadoSalud, EstadoSolicitud, PaginaAuditoria, ReporteUso } from "../types";

// Funcionamiento, utilización y bitácora de auditoría (Reglamento DITIC arts. 2 y 18 c)
export function Operacion() {
  const { sesion, mensajeDeError } = useSesion();
  const [salud, setSalud] = useState<EstadoSalud | null>(null);
  const [reporte, setReporte] = useState<ReporteUso | null>(null);
  const [auditoria, setAuditoria] = useState<PaginaAuditoria | null>(null);
  const [filtro, setFiltro] = useState({ desde: "", hasta: "", accion: "", pagina: 1 });
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    estadoSalud().then(setSalud).catch((e) => setError(mensajeDeError(e)));
    reporteUso(sesion.token).then(setReporte).catch((e) => setError(mensajeDeError(e)));
  }, [sesion.token, mensajeDeError]);

  useEffect(() => {
    consultarAuditoria(filtro, sesion.token)
      .then(setAuditoria)
      .catch((e) => setError(mensajeDeError(e)));
  }, [filtro, sesion.token, mensajeDeError]);

  const buscar = (ev: FormEvent<HTMLFormElement>) => {
    ev.preventDefault();
    const datos = new FormData(ev.currentTarget);
    setFiltro({
      desde: String(datos.get("desde") ?? ""),
      hasta: String(datos.get("hasta") ?? ""),
      accion: String(datos.get("accion") ?? ""),
      pagina: 1,
    });
  };

  const paginas = auditoria ? Math.max(1, Math.ceil(auditoria.total / auditoria.tamanoPagina)) : 1;
  const sano = salud?.estado === "Healthy";

  return (
    <main className="pagina">
      <h1 className="pagina__titulo">Operación</h1>
      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}

      <section className="bloque">
        <h2 className="evidencia__titulo">Disponibilidad</h2>
        {salud && (
          <p className={sano ? "aviso" : "alerta"}>
            {sano ? "Las cinco bases responden." : "Hay bases sin conexión."} Comprobado en {salud.duracionMs} ms:{" "}
            {Object.values(salud.componentes)
              .flatMap((c) => Object.entries(c.bases ?? {}))
              .map(([base, estado]) => `${base} ${estado}`)
              .join(", ")}
            .
          </p>
        )}
      </section>

      {reporte && (
        <section className="bloque">
          <h2 className="evidencia__titulo">Utilización (últimos 30 días)</h2>
          <dl className="meritos">
            <div><dt>Ingresos correctos</dt><dd>{reporte.ingresosExitosos30Dias}</dd></div>
            <div><dt>Ingresos fallidos</dt><dd>{reporte.ingresosFallidos30Dias}</dd></div>
            <div><dt>Operaciones registradas</dt><dd>{reporte.operacionesEscritura30Dias}</dd></div>
            <div><dt>Errores del servidor</dt><dd>{reporte.erroresServidor30Dias}</dd></div>
            <div><dt>Usuarios activos</dt><dd>{reporte.usuariosActivos}</dd></div>
            <div>
              <dt>Días promedio hasta resolver</dt>
              <dd>{reporte.diasPromedioHastaResolucion == null ? "Sin datos" : formatoNumero(reporte.diasPromedioHastaResolucion)}</dd>
            </div>
          </dl>
          <h3 className="grupo-documentos__titulo">Solicitudes por estado</h3>
          <ul className="barras">
            {Object.entries(reporte.solicitudesPorEstado).map(([estado, cantidad]) => {
              const maximo = Math.max(...Object.values(reporte.solicitudesPorEstado), 1);
              return (
                <li key={estado} className="barra">
                  <span className="barra__nombre">{nombreEstado(estado as EstadoSolicitud)}</span>
                  <span className="barra__pista">
                    <span className="barra__relleno" style={{ width: `${(cantidad / maximo) * 100}%` }} />
                  </span>
                  <span className="barra__valor">{cantidad}</span>
                </li>
              );
            })}
          </ul>
        </section>
      )}

      <section className="bloque">
        <h2 className="evidencia__titulo">Bitácora de auditoría</h2>
        <form className="formulario-linea" onSubmit={buscar}>
          <label className="campo"><span className="campo__etiqueta">Desde</span><input type="date" name="desde" /></label>
          <label className="campo"><span className="campo__etiqueta">Hasta</span><input type="date" name="hasta" /></label>
          <label className="campo"><span className="campo__etiqueta">Acción</span><input name="accion" placeholder="Solicitudes.Votar" /></label>
          <button className="boton" type="submit">Filtrar</button>
        </form>

        {auditoria && (
          <>
            <div className="tabla-contenedor">
              <table className="tabla">
                <thead>
                  <tr>
                    <th scope="col">Fecha</th>
                    <th scope="col">Usuario</th>
                    <th scope="col">Acción</th>
                    <th scope="col" className="tabla__num">Resultado</th>
                    <th scope="col">Detalle</th>
                  </tr>
                </thead>
                <tbody>
                  {auditoria.registros.map((r) => (
                    <tr key={r.id}>
                      <td className="tabla__fecha">{formatoFechaHora(r.fecha)}</td>
                      <td>
                        {r.usuario ?? "Anónimo"}
                        <span className="tabla__secundario">{r.ip}</span>
                      </td>
                      <td>
                        {r.accion}
                        <span className="tabla__secundario">
                          {r.metodo} {r.ruta}
                        </span>
                      </td>
                      <td className={`tabla__num ${r.estado >= 400 ? "tabla__num--bajo" : ""}`}>{r.estado}</td>
                      <td className="tabla__detalle">{r.detalle}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="paginacion">
              <button type="button" className="boton-texto" disabled={filtro.pagina <= 1} onClick={() => setFiltro({ ...filtro, pagina: filtro.pagina - 1 })}>
                Anterior
              </button>
              <span>
                Página {filtro.pagina} de {paginas} ({auditoria.total} registros)
              </span>
              <button type="button" className="boton-texto" disabled={filtro.pagina >= paginas} onClick={() => setFiltro({ ...filtro, pagina: filtro.pagina + 1 })}>
                Siguiente
              </button>
            </div>
          </>
        )}
      </section>
    </main>
  );
}
