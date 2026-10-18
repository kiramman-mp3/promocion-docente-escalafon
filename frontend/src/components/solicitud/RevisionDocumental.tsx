import { useId, useState } from "react";
import { enviarAComision, revisarDocumento } from "../../api";
import { nombreTipoDocumento, plural } from "../../format";
import { useSesion } from "../../sesion";
import type { DocumentoSolicitud, Solicitud } from "../../types";
import { ControlRevision } from "../ControlRevision";

// Talento Humano revisa cada documento del expediente y envía la solicitud a la Comisión Académica
export function RevisionDocumental({ solicitud, onCambio }: { solicitud: Solicitud; onCambio: (s: Solicitud) => void }) {
  const { sesion, esRevisor, mensajeDeError } = useSesion();
  const ids = useId();
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const documentos = solicitud.documentos ?? [];
  const editable = esRevisor && solicitud.estado === "RevisionDocumental";
  const revisados = documentos.filter((d) => d.revision).length;
  const rechazados = documentos.filter((d) => d.revision?.estado === "Rechazado").length;
  const grupos = documentos.reduce<Record<string, DocumentoSolicitud[]>>((acc, d) => {
    (acc[d.tipo] ??= []).push(d);
    return acc;
  }, {});

  async function enviar() {
    setEnviando(true);
    setError(null);
    try {
      onCambio(await enviarAComision(solicitud.id, sesion.token));
    } catch (e) {
      setError(mensajeDeError(e));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <section className="bloque" aria-labelledby={`${ids}-titulo`}>
      <h2 id={`${ids}-titulo`} className="evidencia__titulo">
        Revisión documental
      </h2>
      <p className="bloque__ayuda">
        {revisados === documentos.length
          ? `Se revisaron los ${plural(documentos.length, "documento")}`
          : `Revisados ${revisados} de ${documentos.length} documentos`}
        {rechazados > 0 && `, ${rechazados === 1 ? "1 rechazado" : `${rechazados} rechazados`}`}.
        {editable && " Al terminar, envía el expediente a la Comisión Académica, que decide la promoción (UTA art. 47)."}
      </p>

      {Object.entries(grupos).map(([tipo, docs]) => (
        <div key={tipo} className="grupo-documentos">
          <h3 className="grupo-documentos__titulo">{nombreTipoDocumento(tipo)}</h3>
          <ul className="documentos">
            {docs.map((d) => (
              <li key={`${d.tipo}-${d.id}`} className="documento">
                <span className="documento__descripcion">{d.descripcion}</span>
                <ControlRevision
                  revision={d.revision ?? undefined}
                  editable={editable}
                  etiqueta={d.descripcion}
                  onRevisar={async (estado, observacion) => {
                    try {
                      onCambio(await revisarDocumento(solicitud.id, { tipoDocumento: d.tipo, documentoId: d.id, estado, observacion }, sesion.token));
                    } catch (e) {
                      throw new Error(mensajeDeError(e));
                    }
                  }}
                />
              </li>
            ))}
          </ul>
        </div>
      ))}

      {editable && (
        <div className="resolucion__acciones">
          <button type="button" className="boton" disabled={enviando || revisados < documentos.length} onClick={enviar}>
            {enviando ? "Enviando…" : "Enviar a la Comisión Académica"}
          </button>
          {revisados < documentos.length && (
            <span className="bloque__ayuda">Faltan {plural(documentos.length - revisados, "documento")} por revisar.</span>
          )}
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
