import { useState, type FormEvent } from "react";
import { iniciarSesion } from "../api";
import type { Sesion } from "../types";
import { Escalafon } from "./Escalafon";

export function Login({ onSesion }: { onSesion: (s: Sesion) => void }) {
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
        <Escalafon />
      </div>

      <div className="login__contenido">
        <p className="marca">Universidad Técnica de Ambato</p>
        <h1 className="login__titulo">Promoción docente</h1>
        <p className="login__intro">
          Revisa si un docente cumple los requisitos para subir al siguiente rol del escalafón, con los
          documentos de RRHH, DIDE, DEaDV y DAC en un solo expediente.
        </p>

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
          <p className="login__ayuda">
            Entorno de desarrollo: usa <strong>ana.perez@uta.edu.ec</strong> con la contraseña <strong>Uta2026*</strong>.
          </p>
        )}
      </div>
    </main>
  );
}
