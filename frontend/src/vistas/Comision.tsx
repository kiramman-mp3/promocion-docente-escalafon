import { useCallback, useEffect, useState, type FormEvent } from "react";
import { agregarMiembroComision, finalizarMiembroComision, listarMiembrosComision, listarSolicitudes, listarUsuarios } from "../api";
import { EtiquetaEstado } from "../components/EtiquetaEstado";
import { formatoFecha, nombreCargo } from "../format";
import { enlace } from "../ruta";
import { useSesion } from "../sesion";
import type { CargoComision, MiembroComision, Solicitud, Usuario } from "../types";

const CARGOS: CargoComision[] = ["VicerrectorAcademico", "ProfesorHCU", "ProfesorTerna", "DirectorFinanciero", "DirectorTalentoHumano"];

// Comisión Académica de promoción (UTA 0153-HCU-2026, art. 47)
export function Comision() {
  const { sesion, mensajeDeError } = useSesion();
  const esAdmin = sesion.rol === "Administrador";
  const [porVotar, setPorVotar] = useState<Solicitud[] | null>(null);
  const [miembros, setMiembros] = useState<MiembroComision[] | null>(null);
  const [historico, setHistorico] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const cargarMiembros = useCallback(() => {
    listarMiembrosComision(sesion.token, historico)
      .then(setMiembros)
      .catch((e) => setError(mensajeDeError(e)));
  }, [sesion.token, historico, mensajeDeError]);

  useEffect(() => {
    if (sesion.esMiembroComision || esAdmin)
      listarSolicitudes(sesion.token, "EnComision")
        .then(setPorVotar)
        .catch((e) => setError(mensajeDeError(e)));
  }, [sesion.token, sesion.esMiembroComision, esAdmin, mensajeDeError]);

  useEffect(cargarMiembros, [cargarMiembros]);

  return (
    <main className="pagina">
      <h1 className="pagina__titulo">Comisión Académica</h1>
      <p className="pagina__intro">
        Responsable de la promoción del personal académico titular. La preside el Vicerrector Académico, con voto dirimente; la integran un
        profesor designado por el HCU, dos profesores de las ternas, el Director Financiero y el Director de Talento Humano (UTA art. 47).
      </p>

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}

      <section className="bloque">
        <h2 className="evidencia__titulo">Solicitudes en votación</h2>
        {!porVotar && <p className="cargando">Cargando…</p>}
        {porVotar?.length === 0 && <p className="evidencia__vacio">No hay solicitudes esperando la votación de la Comisión.</p>}
        {!!porVotar?.length && (
          <ul className="lista-simple">
            {porVotar.map((s) => (
              <li key={s.id}>
                <span>
                  <strong>
                    {s.rolActual} a {s.rolObjetivo}
                  </strong>
                  <span className="tabla__secundario">
                    Cédula {s.cedula}, enviada el {s.fechaEnvioComision ? formatoFecha(s.fechaEnvioComision) : "-"}
                  </span>
                </span>
                <span className="lista-simple__acciones">
                  <EtiquetaEstado estado={s.estado} />
                  <a className="boton-pdf" href={enlace({ vista: "solicitud", id: s.id })}>
                    {sesion.esMiembroComision ? "Revisar y votar" : "Ver"}
                  </a>
                </span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="bloque">
        <div className="pagina__encabezado">
          <h2 className="evidencia__titulo">Integrantes</h2>
          <label className="opcion">
            <input type="checkbox" checked={historico} onChange={(e) => setHistorico(e.target.checked)} />
            Incluir integrantes anteriores
          </label>
        </div>
        {miembros && (
          <ul className="lista-simple">
            {miembros.map((m) => (
              <li key={m.id} className={m.vigenteHasta ? "lista-simple__inactivo" : ""}>
                <span>
                  <strong>
                    {m.nombre}
                    {m.esPresidente && ", preside"}
                  </strong>
                  <span className="tabla__secundario">
                    {nombreCargo(m.cargo)}, desde {formatoFecha(m.vigenteDesde)}
                    {m.vigenteHasta && ` hasta ${formatoFecha(m.vigenteHasta)}`}
                  </span>
                </span>
                {esAdmin && !m.vigenteHasta && (
                  <button
                    type="button"
                    className="boton-texto"
                    onClick={() =>
                      finalizarMiembroComision(m.id, sesion.token)
                        .then(cargarMiembros)
                        .catch((e) => setError(mensajeDeError(e)))
                    }
                  >
                    Finalizar membresía
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
        {esAdmin && <NuevoMiembro onAgregado={cargarMiembros} />}
      </section>
    </main>
  );
}

function NuevoMiembro({ onAgregado }: { onAgregado: () => void }) {
  const { sesion, mensajeDeError } = useSesion();
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [usuarioId, setUsuarioId] = useState(0);
  const [cargo, setCargo] = useState<CargoComision>("ProfesorTerna");
  const [esPresidente, setEsPresidente] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    listarUsuarios(sesion.token)
      .then((u) => setUsuarios(u.filter((x) => x.activo)))
      .catch((e) => setError(mensajeDeError(e)));
  }, [sesion.token, mensajeDeError]);

  async function agregar(ev: FormEvent) {
    ev.preventDefault();
    setError(null);
    try {
      await agregarMiembroComision({ usuarioId, cargo, esPresidente: cargo === "VicerrectorAcademico" && esPresidente }, sesion.token);
      setUsuarioId(0);
      onAgregado();
    } catch (e) {
      setError(mensajeDeError(e));
    }
  }

  return (
    <form className="formulario-linea" onSubmit={agregar}>
      <label className="campo campo--ancho">
        <span className="campo__etiqueta">Usuario</span>
        <select value={usuarioId} onChange={(e) => setUsuarioId(Number(e.target.value))}>
          <option value={0}>Elige un usuario</option>
          {usuarios.map((u) => (
            <option key={u.id} value={u.id}>
              {u.nombre} {u.apellido} ({u.correo})
            </option>
          ))}
        </select>
      </label>
      <label className="campo">
        <span className="campo__etiqueta">Cargo</span>
        <select value={cargo} onChange={(e) => setCargo(e.target.value as CargoComision)}>
          {CARGOS.map((c) => (
            <option key={c} value={c}>
              {nombreCargo(c)}
            </option>
          ))}
        </select>
      </label>
      {cargo === "VicerrectorAcademico" && (
        <label className="opcion">
          <input type="checkbox" checked={esPresidente} onChange={(e) => setEsPresidente(e.target.checked)} />
          Preside
        </label>
      )}
      <button className="boton" type="submit" disabled={!usuarioId}>
        Agregar integrante
      </button>
      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
    </form>
  );
}
