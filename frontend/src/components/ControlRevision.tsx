import { useId, useState, type FormEvent } from "react";
import type { EstadoRevision, RevisionDocumento } from "../types";

interface Props {
  revision?: RevisionDocumento;
  editable: boolean;
  etiqueta: string; // nombre del documento, para lectores de pantalla
  onRevisar: (estado: EstadoRevision, observacion?: string) => Promise<void>;
}

export function ControlRevision({ revision, editable, etiqueta, onRevisar }: Props) {
  const idMotivo = useId();
  const [rechazando, setRechazando] = useState(false);
  const [motivo, setMotivo] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function guardar(estado: EstadoRevision, observacion?: string) {
    setGuardando(true);
    setError(null);
    try {
      await onRevisar(estado, observacion);
      setRechazando(false);
      setMotivo("");
    } catch (e) {
      setError(e instanceof Error ? e.message : "No se pudo guardar la revisión.");
    } finally {
      setGuardando(false);
    }
  }

  function confirmarRechazo(ev: FormEvent) {
    ev.preventDefault();
    if (motivo.trim()) guardar("Rechazado", motivo.trim());
  }

  return (
    <div className="revision">
      <p className={`revision__estado revision__estado--${revision?.estado.toLowerCase() ?? "pendiente"}`}>
        {revision ? revision.estado : "Sin revisar"}
      </p>
      {revision?.observacion && <p className="revision__motivo">{revision.observacion}</p>}

      {editable && !rechazando && (
        <div className="revision__acciones">
          <button
            type="button"
            className="boton-chico boton-chico--aprobar"
            disabled={guardando || revision?.estado === "Aprobado"}
            onClick={() => guardar("Aprobado")}
            aria-label={`Aprobar ${etiqueta}`}
          >
            Aprobar
          </button>
          <button
            type="button"
            className="boton-chico boton-chico--rechazar"
            disabled={guardando}
            onClick={() => setRechazando(true)}
            aria-label={`Rechazar ${etiqueta}`}
          >
            Rechazar
          </button>
        </div>
      )}

      {editable && rechazando && (
        <form className="revision__rechazo" onSubmit={confirmarRechazo}>
          <label htmlFor={idMotivo} className="sr-only">
            Motivo del rechazo de {etiqueta}
          </label>
          <input
            id={idMotivo}
            autoFocus
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            placeholder="Motivo del rechazo"
            maxLength={500}
          />
          <div className="revision__acciones">
            <button type="submit" className="boton-chico boton-chico--rechazar" disabled={guardando || !motivo.trim()}>
              Confirmar rechazo
            </button>
            <button type="button" className="boton-texto" onClick={() => setRechazando(false)}>
              Cancelar
            </button>
          </div>
        </form>
      )}

      {error && (
        <p className="revision__error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
