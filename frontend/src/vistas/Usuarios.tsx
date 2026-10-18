import { useEffect, useState, type FormEvent } from "react";
import { actualizarUsuario, cambiarContrasena, crearUsuario, listarUsuarios, type DatosUsuario } from "../api";
import { useSesion } from "../sesion";
import type { RolSistema, Usuario } from "../types";

const ROLES: RolSistema[] = ["Docente", "RRHH", "Administrador"];

// Campos compartidos por el alta y la edición
function CamposUsuario({ datos, onCambio }: { datos: DatosUsuario; onCambio: (d: DatosUsuario) => void }) {
  return (
    <>
      <label className="campo">
        <span className="campo__etiqueta">Nombre</span>
        <input required value={datos.nombre} onChange={(e) => onCambio({ ...datos, nombre: e.target.value })} />
      </label>
      <label className="campo">
        <span className="campo__etiqueta">Apellido</span>
        <input required value={datos.apellido} onChange={(e) => onCambio({ ...datos, apellido: e.target.value })} />
      </label>
      <label className="campo">
        <span className="campo__etiqueta">Rol</span>
        <select value={datos.rol} onChange={(e) => onCambio({ ...datos, rol: e.target.value as RolSistema })}>
          {ROLES.map((r) => (
            <option key={r}>{r}</option>
          ))}
        </select>
      </label>
      <label className="campo">
        <span className="campo__etiqueta">Cédula {datos.rol === "Docente" ? "" : "(opcional)"}</span>
        <input
          inputMode="numeric"
          maxLength={10}
          required={datos.rol === "Docente"}
          value={datos.cedula ?? ""}
          onChange={(e) => onCambio({ ...datos, cedula: e.target.value.replace(/\D/g, "") || null })}
        />
      </label>
    </>
  );
}

function NuevoUsuario({ onCreado }: { onCreado: (u: Usuario) => void }) {
  const { sesion, mensajeDeError } = useSesion();
  const vacio: DatosUsuario & { correo: string; contrasena: string } = {
    correo: "",
    contrasena: "",
    nombre: "",
    apellido: "",
    rol: "Docente",
    cedula: null,
  };
  const [datos, setDatos] = useState(vacio);
  const [abierto, setAbierto] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function crear(ev: FormEvent) {
    ev.preventDefault();
    setEnviando(true);
    setError(null);
    try {
      onCreado(await crearUsuario(datos, sesion.token));
      setDatos(vacio);
      setAbierto(false);
    } catch (e) {
      setError(mensajeDeError(e));
    } finally {
      setEnviando(false);
    }
  }

  if (!abierto) {
    return (
      <button type="button" className="boton" onClick={() => setAbierto(true)}>
        Crear usuario
      </button>
    );
  }

  return (
    <form className="formulario-usuario" onSubmit={crear}>
      <h2 className="evidencia__titulo">Nuevo usuario</h2>
      <div className="formulario-usuario__campos">
        <label className="campo">
          <span className="campo__etiqueta">Correo institucional</span>
          <input type="email" required value={datos.correo} onChange={(e) => setDatos({ ...datos, correo: e.target.value })} />
        </label>
        <label className="campo">
          <span className="campo__etiqueta">Contraseña inicial</span>
          <input
            type="password"
            autoComplete="new-password"
            minLength={8}
            required
            value={datos.contrasena}
            onChange={(e) => setDatos({ ...datos, contrasena: e.target.value })}
          />
        </label>
        <CamposUsuario datos={datos} onCambio={(d) => setDatos({ ...datos, ...d })} />
      </div>
      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
      <div className="resolucion__acciones">
        <button className="boton" type="submit" disabled={enviando}>
          {enviando ? "Creando…" : "Crear usuario"}
        </button>
        <button type="button" className="boton-texto" onClick={() => setAbierto(false)}>
          Cancelar
        </button>
      </div>
    </form>
  );
}

function FilaUsuario({ usuario, onActualizado }: { usuario: Usuario; onActualizado: (u: Usuario) => void }) {
  const { sesion, mensajeDeError } = useSesion();
  const [modo, setModo] = useState<"ver" | "editar" | "contrasena">("ver");
  const [datos, setDatos] = useState<DatosUsuario & { activo: boolean }>(usuario);
  const [contrasena, setContrasena] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [aviso, setAviso] = useState<{ error: boolean; texto: string } | null>(null);
  const esUnoMismo = usuario.id === sesion.id;

  async function ejecutar(accion: () => Promise<void>) {
    setEnviando(true);
    setAviso(null);
    try {
      await accion();
    } catch (e) {
      setAviso({ error: true, texto: mensajeDeError(e) });
    } finally {
      setEnviando(false);
    }
  }

  const guardar = (ev: FormEvent) => {
    ev.preventDefault();
    ejecutar(async () => {
      onActualizado(await actualizarUsuario(usuario.id, datos, sesion.token));
      setModo("ver");
      setAviso({ error: false, texto: "Cambios guardados." });
    });
  };

  const restablecer = (ev: FormEvent) => {
    ev.preventDefault();
    ejecutar(async () => {
      await cambiarContrasena(usuario.id, contrasena, sesion.token);
      setContrasena("");
      setModo("ver");
      setAviso({ error: false, texto: "Contraseña restablecida." });
    });
  };

  const alternarActivo = () =>
    ejecutar(async () => {
      onActualizado(await actualizarUsuario(usuario.id, { ...usuario, activo: !usuario.activo }, sesion.token));
    });

  return (
    <li className={`usuario ${usuario.activo ? "" : "usuario--inactivo"}`}>
      <div className="usuario__resumen">
        <div>
          <p className="usuario__nombre">
            {usuario.nombre} {usuario.apellido}
            {esUnoMismo && <span className="usuario__tu"> (tú)</span>}
          </p>
          <p className="usuario__detalle">
            {usuario.correo}
            {usuario.cedula && `, cédula ${usuario.cedula}`}
          </p>
        </div>
        <p className="usuario__rol">{usuario.rol}</p>
        <p className={`usuario__estado ${usuario.activo ? "" : "usuario__estado--inactivo"}`}>
          {usuario.activo ? "Activo" : "Desactivado"}
        </p>
        <div className="usuario__acciones">
          <button type="button" className="boton-texto" onClick={() => { setDatos(usuario); setModo(modo === "editar" ? "ver" : "editar"); }}>
            Editar
          </button>
          <button type="button" className="boton-texto" onClick={() => setModo(modo === "contrasena" ? "ver" : "contrasena")}>
            Restablecer contraseña
          </button>
          {!esUnoMismo && (
            <button type="button" className="boton-texto" disabled={enviando} onClick={alternarActivo}>
              {usuario.activo ? "Desactivar" : "Activar"}
            </button>
          )}
        </div>
      </div>

      {modo === "editar" && (
        <form className="usuario__formulario" onSubmit={guardar}>
          <div className="formulario-usuario__campos">
            <CamposUsuario datos={datos} onCambio={(d) => setDatos({ ...datos, ...d })} />
          </div>
          <div className="resolucion__acciones">
            <button className="boton" type="submit" disabled={enviando}>
              Guardar cambios
            </button>
            <button type="button" className="boton-texto" onClick={() => setModo("ver")}>
              Cancelar
            </button>
          </div>
        </form>
      )}

      {modo === "contrasena" && (
        <form className="usuario__formulario usuario__formulario--linea" onSubmit={restablecer}>
          <label className="campo">
            <span className="campo__etiqueta">Nueva contraseña (mínimo 8 caracteres)</span>
            <input
              type="password"
              autoComplete="new-password"
              minLength={8}
              required
              value={contrasena}
              onChange={(e) => setContrasena(e.target.value)}
            />
          </label>
          <button className="boton" type="submit" disabled={enviando || contrasena.length < 8}>
            Restablecer
          </button>
        </form>
      )}

      {aviso && (
        <p className={aviso.error ? "alerta" : "aviso"} role={aviso.error ? "alert" : "status"}>
          {aviso.texto}
        </p>
      )}
    </li>
  );
}

export function Usuarios() {
  const { sesion, mensajeDeError } = useSesion();
  const [usuarios, setUsuarios] = useState<Usuario[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    listarUsuarios(sesion.token)
      .then(setUsuarios)
      .catch((e) => setError(mensajeDeError(e)));
  }, [sesion.token, mensajeDeError]);

  const reemplazar = (u: Usuario) => setUsuarios((lista) => lista?.map((x) => (x.id === u.id ? u : x)) ?? null);

  return (
    <main className="pagina">
      <div className="pagina__encabezado">
        <h1 className="pagina__titulo">Usuarios</h1>
        <NuevoUsuario onCreado={(u) => setUsuarios((lista) => [u, ...(lista ?? [])])} />
      </div>

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
      {!usuarios && !error && <p className="cargando">Cargando usuarios…</p>}

      {usuarios && (
        <ul className="usuarios">
          {usuarios.map((u) => (
            <FilaUsuario key={u.id} usuario={u} onActualizado={reemplazar} />
          ))}
        </ul>
      )}
    </main>
  );
}
