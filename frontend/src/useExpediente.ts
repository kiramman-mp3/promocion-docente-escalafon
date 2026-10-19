import { useCallback, useEffect, useState } from "react";
import { ApiError, cargarExpediente } from "./api";
import { useSesion } from "./sesion";
import type { Dictamen, Expediente } from "./types";

// Carga el expediente de una cédula; con cedula = null no hace nada.
export function useExpediente(cedula: string | null) {
  const { sesion, mensajeDeError } = useSesion();
  const [expediente, setExpediente] = useState<Expediente | null>(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const cargar = useCallback(
    async (c: string) => {
      setCargando(true);
      setError(null);
      try {
        setExpediente(await cargarExpediente(c, sesion.token));
      } catch (e) {
        setExpediente(null);
        setError(
          e instanceof ApiError && e.status === 404
            ? `RRHH no tiene registrada la cédula ${c}. Revisa el número e intenta de nuevo.`
            : mensajeDeError(e),
        );
      } finally {
        setCargando(false);
      }
    },
    [sesion.token, mensajeDeError],
  );

  useEffect(() => {
    if (cedula) cargar(cedula);
  }, [cedula, cargar]);

  return { expediente, cargando, error, recargar: () => cedula && cargar(cedula) };
}

export function cumpleTodo(dictamen: Dictamen | null | undefined) {
  return !!dictamen?.promocionable && dictamen.cumpleTodo;
}
