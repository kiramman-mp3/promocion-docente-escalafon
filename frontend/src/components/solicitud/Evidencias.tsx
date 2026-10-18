import { useEffect, useId, useState, type FormEvent } from "react";
import { abrirPdfProtegido, listarEvidencias, subirEvidencia } from "../../api";
import { formatoFecha } from "../../format";
import { useSesion } from "../../sesion";
import type { EvidenciaArchivo, Solicitud } from "../../types";

const TIPOS = [
  ["CertificadoCongreso", "Certificado de congreso"],
  ["Capacitacion", "Certificado de capacitación"],
  ["Equivalencia", "Equivalencia (evaluador, facilitador)"],
  ["Sabatico", "Informe de periodo sabático"],
  ["CargoAutoridad", "Nombramiento de autoridad"],
  ["Obra", "Obra o publicación"],
  ["Proyecto", "Proyecto"],
  ["Otro", "Otro"],
] as const;

// Evidencias que el docente anexa a su solicitud (CES art. 99)
export function Evidencias({ solicitud, onCambio }: { solicitud: Solicitud; onCambio: () => void }) {
  const { sesion, esRevisor, mensajeDeError } = useSesion();
  const ids = useId();
  const [evidencias, setEvidencias] = useState<EvidenciaArchivo[]>([]);
  const [archivo, setArchivo] = useState<File | null>(null);
  const [tipo, setTipo] = useState<string>("CertificadoCongreso");
  const [descripcion, setDescripcion] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    listarEvidencias(solicitud.id, sesion.token)
      .then(setEvidencias)
      .catch((e) => setError(mensajeDeError(e)));
  }, [solicitud.id, sesion.token, mensajeDeError]);

  const puedeSubir =
    solicitud.estado === "RevisionDocumental" && (esRevisor || sesion.cedula === solicitud.cedula);

  async function subir(ev: FormEvent<HTMLFormElement>) {
    ev.preventDefault();
    const formulario = ev.currentTarget;
    if (!archivo) return;
    if (archivo.size > 10 * 1024 * 1024) {
      setError("El archivo pesa más de 10 MB.");
      return;
    }
    setEnviando(true);
    setError(null);
    try {
      const nueva = await subirEvidencia(solicitud.id, archivo, tipo, descripcion.trim(), sesion.token);
      setEvidencias((lista) => [...lista, nueva]);
      setArchivo(null);
      setDescripcion("");
      formulario.reset();
      onCambio();
    } catch (e) {
      setError(mensajeDeError(e));
    } finally {
      setEnviando(false);
    }
  }

  if (!puedeSubir && evidencias.length === 0) return null;

  return (
    <section className="bloque" aria-labelledby={`${ids}-titulo`}>
      <h2 id={`${ids}-titulo`} className="evidencia__titulo">
        Evidencias anexadas
      </h2>
      <p className="bloque__ayuda">
        Certificados que no están en las bases institucionales (congresos, equivalencias, sabático). Solo PDF de hasta 10 MB (CES art. 99).
      </p>

      {evidencias.length > 0 && (
        <ul className="lista-simple">
          {evidencias.map((e) => (
            <li key={e.id}>
              <span>
                <strong>{e.descripcion}</strong>
                <span className="tabla__secundario">
                  {e.nombreArchivo}, subido por {e.subidoPor} el {formatoFecha(e.fechaSubida)}
                </span>
              </span>
              <button
                type="button"
                className="boton-pdf"
                onClick={() => abrirPdfProtegido(`/api/solicitudes/${solicitud.id}/evidencias/${e.id}`, sesion.token).catch((x) => setError(mensajeDeError(x)))}
              >
                Ver PDF
              </button>
            </li>
          ))}
        </ul>
      )}

      {puedeSubir && (
        <form className="formulario-linea" onSubmit={subir}>
          <label className="campo">
            <span className="campo__etiqueta">Tipo</span>
            <select value={tipo} onChange={(e) => setTipo(e.target.value)}>
              {TIPOS.map(([valor, nombre]) => (
                <option key={valor} value={valor}>
                  {nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="campo campo--ancho">
            <span className="campo__etiqueta">Qué demuestra</span>
            <input value={descripcion} onChange={(e) => setDescripcion(e.target.value)} maxLength={300} required />
          </label>
          <label className="campo">
            <span className="campo__etiqueta">Archivo PDF</span>
            <input type="file" accept="application/pdf" required onChange={(e) => setArchivo(e.target.files?.[0] ?? null)} />
          </label>
          <button className="boton" type="submit" disabled={enviando || !archivo || !descripcion.trim()}>
            {enviando ? "Subiendo…" : "Anexar evidencia"}
          </button>
        </form>
      )}

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
    </section>
  );
}
