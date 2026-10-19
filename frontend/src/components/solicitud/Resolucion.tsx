import { useId, useState, type FormEvent } from "react";
import { abrirPdfProtegido, apelar, registrarAccionPersonal, resolverApelacion } from "../../api";
import { formatoFecha } from "../../format";
import { useSesion } from "../../sesion";
import type { Solicitud } from "../../types";

// Resolución (acto administrativo, CES art. 77 l), ejecución por Talento Humano y apelación (COA arts. 217-224)
export function Resolucion({ solicitud: s, onCambio }: { solicitud: Solicitud; onCambio: (s: Solicitud) => void }) {
  const { sesion, esRevisor, mensajeDeError } = useSesion();
  const ids = useId();
  const [numeroAccion, setNumeroAccion] = useState("");
  const [fechaAccion, setFechaAccion] = useState(new Date().toISOString().slice(0, 10));
  const [fundamentos, setFundamentos] = useState("");
  const [resolucionHCU, setResolucionHCU] = useState("");
  const [motivacion, setMotivacion] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!s.numeroResolucion && !s.apelacion) return null;

  async function ejecutar(accion: () => Promise<Solicitud>) {
    setEnviando(true);
    setError(null);
    try {
      onCambio(await accion());
    } catch (e) {
      setError(mensajeDeError(e));
    } finally {
      setEnviando(false);
    }
  }

  const registrar = (ev: FormEvent) => {
    ev.preventDefault();
    ejecutar(() => registrarAccionPersonal(s.id, numeroAccion.trim(), fechaAccion, sesion.token));
  };
  const presentarApelacion = (ev: FormEvent) => {
    ev.preventDefault();
    ejecutar(() => apelar(s.id, fundamentos.trim(), sesion.token));
  };
  const resolver = (aceptada: boolean) =>
    ejecutar(() => resolverApelacion(s.id, { aceptada, resolucionHCU: resolucionHCU.trim(), motivacion: motivacion.trim() }, sesion.token));

  return (
    <section className="bloque" aria-labelledby={`${ids}-titulo`}>
      <h2 id={`${ids}-titulo`} className="evidencia__titulo">
        Resolución
      </h2>

      {s.numeroResolucion && (
        <div className="acto">
          <p>
            Resolución <strong>{s.numeroResolucion}</strong>
            {s.fechaResolucion && ` del ${formatoFecha(s.fechaResolucion)}`}
            {s.resueltoPor && `, firmada por ${s.resueltoPor}`}.
          </p>
          {s.observacion && <p className="acto__motivacion">{s.observacion}</p>}
          {s.actoDisponible && (
            <p className="acto__acciones">
              <button
                type="button"
                className="boton-pdf"
                onClick={() => abrirPdfProtegido(`/api/solicitudes/${s.id}/acto`, sesion.token).catch((e) => setError(mensajeDeError(e)))}
              >
                Descargar acto administrativo (PDF)
              </button>
              <span className="acto__huella" title="Huella SHA-256 para verificar que el documento no fue alterado">
                SHA-256 {s.actoHash?.slice(0, 16)}…
              </span>
            </p>
          )}
        </div>
      )}

      {s.estado === "Ejecutada" && (
        <p className="aviso">
          Ejecutada con la acción de personal {s.accionPersonalNumero}
          {s.accionPersonalFecha && ` del ${formatoFecha(s.accionPersonalFecha)}`}.
        </p>
      )}

      {s.estado === "Aprobada" && esRevisor && (
        <form className="formulario-linea" onSubmit={registrar}>
          <label className="campo">
            <span className="campo__etiqueta">N.º de acción de personal</span>
            <input value={numeroAccion} onChange={(e) => setNumeroAccion(e.target.value)} maxLength={50} required />
          </label>
          <label className="campo">
            <span className="campo__etiqueta">Fecha</span>
            <input type="date" value={fechaAccion} onChange={(e) => setFechaAccion(e.target.value)} required />
          </label>
          <button className="boton" type="submit" disabled={enviando || !numeroAccion.trim()}>
            Registrar acción de personal
          </button>
        </form>
      )}

      {s.apelacion && (
        <div className="apelacion">
          <h3 className="grupo-documentos__titulo">Apelación</h3>
          <p>
            Presentada el {formatoFecha(s.apelacion.fechaPresentacion)}: {s.apelacion.fundamentos}
          </p>
          {s.apelacion.estado !== "Pendiente" && (
            <p className={s.apelacion.estado === "Aceptada" ? "aviso" : "alerta"}>
              El Consejo Universitario la {s.apelacion.estado === "Aceptada" ? "aceptó" : "negó"} (resolución {s.apelacion.resolucionHCU}):{" "}
              {s.apelacion.motivacion}
            </p>
          )}
        </div>
      )}

      {s.puedeApelar && (
        <form className="formulario-voto" onSubmit={presentarApelacion}>
          <label className="campo">
            <span className="campo__etiqueta">
              Fundamentos de la apelación (puedes apelar hasta el {s.plazoApelacionHasta && formatoFecha(s.plazoApelacionHasta)})
            </span>
            <textarea rows={3} maxLength={2000} value={fundamentos} onChange={(e) => setFundamentos(e.target.value)} />
          </label>
          <button className="boton boton--secundario" type="submit" disabled={enviando || !fundamentos.trim()}>
            Presentar apelación
          </button>
        </form>
      )}

      {s.estado === "Rechazada" && !s.apelacion && !s.puedeApelar && s.plazoApelacionHasta && sesion.cedula === s.cedula && (
        <p className="bloque__ayuda">El término para apelar venció el {formatoFecha(s.plazoApelacionHasta)}.</p>
      )}

      {s.estado === "Apelada" && sesion.rol === "Administrador" && (
        <div className="formulario-voto">
          <p className="bloque__ayuda">Registra lo que resolvió el Honorable Consejo Universitario. Esta decisión es definitiva.</p>
          <label className="campo">
            <span className="campo__etiqueta">N.º de resolución del HCU</span>
            <input value={resolucionHCU} onChange={(e) => setResolucionHCU(e.target.value)} maxLength={50} />
          </label>
          <label className="campo">
            <span className="campo__etiqueta">Motivación</span>
            <textarea rows={3} maxLength={2000} value={motivacion} onChange={(e) => setMotivacion(e.target.value)} />
          </label>
          <div className="resolucion__acciones">
            <button type="button" className="boton boton--aprobar" disabled={enviando || !resolucionHCU.trim() || !motivacion.trim()} onClick={() => resolver(true)}>
              Aceptar apelación y aprobar
            </button>
            <button type="button" className="boton boton--secundario" disabled={enviando || !resolucionHCU.trim() || !motivacion.trim()} onClick={() => resolver(false)}>
              Negar apelación
            </button>
          </div>
        </div>
      )}

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
    </section>
  );
}
