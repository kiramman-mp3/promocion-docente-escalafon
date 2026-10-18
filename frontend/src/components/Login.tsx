import { useState, type FormEvent } from "react";
import { iniciarSesion } from "../api";
import type { RolEscalafon, Sesion } from "../types";
import { Escalafon } from "./Escalafon";

interface Props {
  roles: RolEscalafon[];
  errorRoles: string | null;
  onSesion: (s: Sesion) => void;
}

const USUARIOS_DE_PRUEBA = [
  { correo: "ana.perez@uta.edu.ec", rol: "Docente" },
  { correo: "rrhh@uta.edu.ec", rol: "RRHH" },
  { correo: "admin@uta.edu.ec", rol: "Administrador" },
  { correo: "vicerrectorado@uta.edu.ec", rol: "Autoridad, preside la Comisión" },
  { correo: "financiero@uta.edu.ec", rol: "Autoridad, Director Financiero" },
  { correo: "lucia.mora@uta.edu.ec", rol: "Docente y miembro de la Comisión" },
];

export function Login({ roles, errorRoles, onSesion }: Props) {
  const [correo, setCorreo] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function enviar(ev: FormEvent) {
    ev.preventDefault();
    setError(null);
    setEnviando(true);
    try {
      onSesion(await iniciarSesion(correo.trim(), contrasena));
    } catch (e) {
      setError(e instanceof Error ? e.message : "No se pudo iniciar sesión.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <main className="login">
      <div className="login__escalera" aria-hidden="true">
        <Escalafon roles={roles} />
      </div>

      <div className="login__contenido">
        <p className="marca">Universidad Técnica de Ambato</p>
        <h1 className="login__titulo">Promoción docente</h1>
        <p className="login__intro">
          Los docentes revisan su expediente y solicitan su promoción. RRHH aprueba cada documento y decide la
          solicitud. Todos ingresan con su correo institucional.
        </p>

        {errorRoles && (
          <p className="alerta" role="alert">
            {errorRoles}
          </p>
        )}

        <form className="formulario" onSubmit={enviar} noValidate>
          <label className="campo">
            <span className="campo__etiqueta">Correo institucional</span>
            <input
              type="email"
              autoComplete="username"
              required
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
              placeholder="nombre@uta.edu.ec"
            />
          </label>
          <label className="campo">
            <span className="campo__etiqueta">Contraseña</span>
            <input
              type="password"
              autoComplete="current-password"
              required
              value={contrasena}
              onChange={(e) => setContrasena(e.target.value)}
            />
          </label>

          {error && (
            <p className="alerta" role="alert">
              {error === "Credenciales inválidas" ? "El correo o la contraseña no coinciden." : error}
            </p>
          )}

          <button className="boton" type="submit" disabled={enviando || !correo || !contrasena}>
            {enviando ? "Ingresando…" : "Ingresar"}
          </button>
        </form>

        {import.meta.env.DEV && (
          <div className="login__ayuda">
            <p>Entorno de desarrollo, contraseña <strong>Uta2026*</strong>:</p>
            <ul>
              {USUARIOS_DE_PRUEBA.map((u) => (
                <li key={u.correo}>
                  <button
                    type="button"
                    className="boton-texto"
                    onClick={() => {
                      setCorreo(u.correo);
                      setContrasena("Uta2026*");
                    }}
                  >
                    {u.correo}
                  </button>{" "}
                  ({u.rol})
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </main>
  );
}
