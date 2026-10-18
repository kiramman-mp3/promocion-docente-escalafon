import { useCallback, useEffect, useState, type FormEvent } from "react";
import { crearRequisito, listarRequisitos, type NuevoRequisito } from "../api";
import { formatoFecha, formatoNumero } from "../format";
import { useSesion } from "../sesion";
import type { RequisitoPromocion } from "../types";

// Requisitos de promoción versionados (CES arts. 76-77; UTA art. 48). Nunca se editan: se crea una nueva vigencia.
export function Requisitos() {
  const { roles, mensajeDeError } = useSesion();
  const [requisitos, setRequisitos] = useState<RequisitoPromocion[] | null>(null);
  const [historico, setHistorico] = useState(false);
  const [editando, setEditando] = useState<RequisitoPromocion | null>(null);
  const [error, setError] = useState<string | null>(null);

  const cargar = useCallback(() => {
    listarRequisitos(historico)
      .then(setRequisitos)
      .catch((e) => setError(mensajeDeError(e)));
  }, [historico, mensajeDeError]);

  useEffect(cargar, [cargar]);

  return (
    <main className="pagina">
      <div className="pagina__encabezado">
        <h1 className="pagina__titulo">Requisitos de promoción</h1>
        <label className="opcion">
          <input type="checkbox" checked={historico} onChange={(e) => setHistorico(e.target.checked)} />
          Ver versiones anteriores
        </label>
      </div>
      <p className="pagina__intro">
        Cada cambio crea una nueva versión con su resolución. Las solicitudes ya presentadas se siguen evaluando con la versión vigente
        cuando se presentaron (CES, transitoria novena). Promoción desde Agregado 3 y Principal 3: no aplica (CES art. 76).
      </p>

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}

      {requisitos && (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Promoción</th>
                <th scope="col" className="tabla__num">Obras</th>
                <th scope="col" className="tabla__num">Evaluación</th>
                <th scope="col" className="tabla__num">Capacitación</th>
                <th scope="col" className="tabla__num">Proyectos</th>
                <th scope="col">Dirección</th>
                <th scope="col" className="tabla__num">Titulación</th>
                <th scope="col">Vigencia</th>
                <th scope="col"><span className="sr-only">Acción</span></th>
              </tr>
            </thead>
            <tbody>
              {requisitos.map((r) => (
                <tr key={r.id} className={r.vigente ? "" : "tabla__fila--tenue"}>
                  <td>
                    {r.rolOrigen} a {r.rolDestino}
                    <span className="tabla__secundario">
                      {r.resolucion}
                      {r.esPropuesta && " (propuesta)"}
                    </span>
                  </td>
                  <td className="tabla__num">
                    {r.obrasTotales}
                    <span className="tabla__secundario">{r.obrasEnGrado} en el grado</span>
                  </td>
                  <td className="tabla__num">
                    {formatoNumero(r.evaluacionMinima)} %<span className="tabla__secundario">últimas {r.evaluacionPeriodos}</span>
                  </td>
                  <td className="tabla__num">
                    {r.capacitacionHoras} h<span className="tabla__secundario">{r.capacitacionAnios} años, {formatoNumero(r.capacitacionPctPedagogica)} % pedag.</span>
                  </td>
                  <td className="tabla__num">{r.proyectosMeses ? `${r.proyectosMeses} meses` : "No"}</td>
                  <td>{r.requiereDireccionProyecto ? "Sí" : "No"}</td>
                  <td className="tabla__num">
                    {r.trabajosTitulacion || "No"}
                    {r.trabajosTitulacion > 0 && <span className="tabla__secundario">{r.titulacionNivelMinimo === "Maestria" ? "Maestría" : r.titulacionNivelMinimo}</span>}
                  </td>
                  <td className="tabla__fecha">
                    {formatoFecha(r.vigenteDesde)}
                    {r.vigenteHasta && <span className="tabla__secundario">hasta {formatoFecha(r.vigenteHasta)}</span>}
                  </td>
                  <td className="tabla__accion">
                    {r.vigente && !r.vigenteHasta && (
                      <button type="button" className="boton-pdf" onClick={() => setEditando(r)}>
                        Nueva versión
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editando && (
        <NuevaVersion
          base={editando}
          nombre={`${roles.find((x) => x.id === editando.gradoOrigen)?.nombre} a ${editando.rolDestino}`}
          onCancelar={() => setEditando(null)}
          onCreado={() => {
            setEditando(null);
            cargar();
          }}
        />
      )}
    </main>
  );
}

function NuevaVersion({
  base,
  nombre,
  onCancelar,
  onCreado,
}: {
  base: RequisitoPromocion;
  nombre: string;
  onCancelar: () => void;
  onCreado: () => void;
}) {
  const { sesion, mensajeDeError } = useSesion();
  const manana = new Date(Date.now() + 86_400_000).toISOString().slice(0, 10);
  const [datos, setDatos] = useState<NuevoRequisito>({ ...base, vigenteDesde: manana, resolucion: "", esPropuesta: false });
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  const numero = (campo: keyof NuevoRequisito) => ({
    type: "number" as const,
    value: String(datos[campo]),
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => setDatos({ ...datos, [campo]: Number(e.target.value) }),
  });

  async function guardar(ev: FormEvent) {
    ev.preventDefault();
    setEnviando(true);
    setError(null);
    try {
      await crearRequisito(datos, sesion.token);
      onCreado();
    } catch (e) {
      setError(mensajeDeError(e));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="formulario-usuario" onSubmit={guardar}>
      <h2 className="evidencia__titulo">Nueva versión: {nombre}</h2>
      <div className="formulario-usuario__campos">
        <label className="campo campo--ancho">
          <span className="campo__etiqueta">Resolución que la aprueba</span>
          <input required value={datos.resolucion} onChange={(e) => setDatos({ ...datos, resolucion: e.target.value })} placeholder="0000-HCU-2026" />
        </label>
        <label className="campo campo--ancho">
          <span className="campo__etiqueta">Normativa</span>
          <input required value={datos.normativa} onChange={(e) => setDatos({ ...datos, normativa: e.target.value })} />
        </label>
        <label className="campo">
          <span className="campo__etiqueta">Vigente desde</span>
          <input type="date" required value={datos.vigenteDesde.slice(0, 10)} onChange={(e) => setDatos({ ...datos, vigenteDesde: e.target.value })} />
        </label>
        <label className="campo"><span className="campo__etiqueta">Obras totales</span><input min={1} {...numero("obrasTotales")} /></label>
        <label className="campo"><span className="campo__etiqueta">Obras en el grado (mín. 1)</span><input min={1} {...numero("obrasEnGrado")} /></label>
        <label className="campo"><span className="campo__etiqueta">Evaluación mínima % (mín. 75)</span><input min={75} max={100} step="0.01" {...numero("evaluacionMinima")} /></label>
        <label className="campo"><span className="campo__etiqueta">Horas de capacitación (mín. 96)</span><input min={96} {...numero("capacitacionHoras")} /></label>
        <label className="campo"><span className="campo__etiqueta">% pedagógico (mín. 25)</span><input min={25} max={100} {...numero("capacitacionPctPedagogica")} /></label>
        <label className="campo"><span className="campo__etiqueta">Meses en proyectos</span><input min={0} {...numero("proyectosMeses")} /></label>
        <label className="campo"><span className="campo__etiqueta">Trabajos de titulación</span><input min={0} {...numero("trabajosTitulacion")} /></label>
        <label className="campo">
          <span className="campo__etiqueta">Nivel mínimo de titulación</span>
          <select value={datos.titulacionNivelMinimo} onChange={(e) => setDatos({ ...datos, titulacionNivelMinimo: e.target.value as NuevoRequisito["titulacionNivelMinimo"] })}>
            <option value="Grado">Grado</option>
            <option value="Maestria">Maestría</option>
            <option value="Doctorado">Doctorado</option>
          </select>
        </label>
        <label className="opcion">
          <input type="checkbox" checked={datos.requiereDireccionProyecto} onChange={(e) => setDatos({ ...datos, requiereDireccionProyecto: e.target.checked })} />
          Exige dirección o codirección de proyectos
        </label>
        <label className="opcion">
          <input type="checkbox" checked={datos.esPropuesta} onChange={(e) => setDatos({ ...datos, esPropuesta: e.target.checked })} />
          Es una propuesta (aún sin normativa interna aprobada)
        </label>
      </div>
      <p className="bloque__ayuda">
        La evaluación se toma de las 4 últimas evaluaciones y la capacitación de los últimos 3 años, como fija el CES (art. 76).
      </p>
      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
      <div className="resolucion__acciones">
        <button className="boton" type="submit" disabled={enviando || !datos.resolucion.trim()}>
          Crear versión
        </button>
        <button type="button" className="boton-texto" onClick={onCancelar}>
          Cancelar
        </button>
      </div>
    </form>
  );
}
