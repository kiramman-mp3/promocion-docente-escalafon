import { useId, useState, type FormEvent } from "react";
import { cerrarVotacion, votar } from "../../api";
import { formatoFechaHora, nombreCargo } from "../../format";
import { useSesion } from "../../sesion";
import type { OpcionVoto, Solicitud } from "../../types";

const OPCIONES: [OpcionVoto, string][] = [
  ["AFavor", "A favor"],
  ["EnContra", "En contra"],
  ["Abstencion", "Abstención"],
];

const nombreVoto = (v: OpcionVoto) => OPCIONES.find(([o]) => o === v)?.[1] ?? v;

// Votación de la Comisión Académica (UTA art. 47): quórum, voto motivado y voto dirimente de la presidencia
export function Votacion({ solicitud, onCambio }: { solicitud: Solicitud; onCambio: (s: Solicitud) => void }) {
  const { sesion, mensajeDeError } = useSesion();
  const ids = useId();
  const v = solicitud.votacion!;
  const [voto, setVoto] = useState<OpcionVoto>(v.miVoto?.voto ?? "AFavor");
  const [motivacion, setMotivacion] = useState(v.miVoto?.motivacion ?? "");
  const [certifica, setCertifica] = useState(v.miVoto?.certificaPresupuesto ?? false);
  const [motivacionCierre, setMotivacionCierre] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const esFinanciero = v.miCargo === "DirectorFinanciero";
  const enComision = solicitud.estado === "EnComision";

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

  const emitir = (ev: FormEvent) => {
    ev.preventDefault();
    ejecutar(() =>
      votar(solicitud.id, { voto, motivacion: motivacion.trim(), ...(esFinanciero ? { certificaPresupuesto: certifica } : {}) }, sesion.token),
    );
  };

  const cerrar = (ev: FormEvent) => {
    ev.preventDefault();
    ejecutar(() => cerrarVotacion(solicitud.id, motivacionCierre.trim(), sesion.token));
  };

  return (
    <section className="bloque" aria-labelledby={`${ids}-titulo`}>
      <h2 id={`${ids}-titulo`} className="evidencia__titulo">
        Comisión Académica
      </h2>
      <p className="bloque__ayuda">
        {v.votosEmitidos} de {v.miembrosActivos} miembros votaron (quórum: {v.quorum}). A favor {v.aFavor}, en contra {v.enContra},
        abstenciones {v.abstenciones}. {v.presupuestoCertificado ? "Presupuesto certificado." : "Falta la certificación presupuestaria del Director Financiero."}
      </p>

      {solicitud.votos && solicitud.votos.length > 0 && (
        <ul className="votos">
          {solicitud.votos.map((x, i) => (
            <li key={i} className={`voto voto--${x.voto.toLowerCase()}`}>
              <span className="voto__quien">
                {x.miembro}
                <span className="tabla__secundario">
                  {nombreCargo(x.cargo)}
                  {x.esPresidente ? ", preside" : ""}
                </span>
              </span>
              <span className="voto__sentido">{nombreVoto(x.voto)}</span>
              <span className="voto__motivo">
                {x.motivacion}
                {x.certificaPresupuesto && <span className="tabla__secundario">Certifica la disponibilidad presupuestaria.</span>}
                <span className="tabla__secundario">{formatoFechaHora(x.fecha)}</span>
              </span>
            </li>
          ))}
        </ul>
      )}

      {enComision && v.puedeVotar && (
        <form className="formulario-voto" onSubmit={emitir}>
          <fieldset className="opciones">
            <legend className="campo__etiqueta">{v.miVoto ? "Cambiar mi voto" : "Mi voto"}</legend>
            {OPCIONES.map(([valor, nombre]) => (
              <label key={valor} className="opcion">
                <input type="radio" name={`${ids}-voto`} value={valor} checked={voto === valor} onChange={() => setVoto(valor)} />
                {nombre}
              </label>
            ))}
          </fieldset>
          <label className="campo">
            <span className="campo__etiqueta">Motivación (obligatoria)</span>
            <textarea rows={2} maxLength={1000} value={motivacion} onChange={(e) => setMotivacion(e.target.value)} />
          </label>
          {esFinanciero && (
            <label className="opcion">
              <input type="checkbox" checked={certifica} onChange={(e) => setCertifica(e.target.checked)} />
              Certifico la disponibilidad presupuestaria para esta promoción
            </label>
          )}
          <button className="boton" type="submit" disabled={enviando || !motivacion.trim()}>
            {v.miVoto ? "Actualizar voto" : "Registrar voto"}
          </button>
        </form>
      )}

      {enComision && !v.puedeVotar && v.motivoNoPuedeVotar && v.esMiembro && <p className="bloque__ayuda">{v.motivoNoPuedeVotar}</p>}

      {v.puedeCerrar && (
        <form className="formulario-voto" onSubmit={cerrar}>
          <label className="campo">
            <span className="campo__etiqueta">Motivación de la resolución (Código Orgánico Administrativo, art. 100)</span>
            <textarea rows={3} maxLength={1500} value={motivacionCierre} onChange={(e) => setMotivacionCierre(e.target.value)} />
          </label>
          <button className="boton boton--secundario" type="submit" disabled={enviando || !motivacionCierre.trim() || v.votosEmitidos < v.quorum}>
            Cerrar votación y emitir resolución
          </button>
          {v.votosEmitidos < v.quorum && <span className="bloque__ayuda">Aún no hay quórum.</span>}
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
