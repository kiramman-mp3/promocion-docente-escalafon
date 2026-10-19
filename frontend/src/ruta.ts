import { useEffect, useState } from "react";

// Rutas por hash (#/solicitudes/3) para que el botón "atrás" funcione sin servidor de rutas.
export type Ruta =
  | { vista: "expediente" }
  | { vista: "consulta" }
  | { vista: "solicitudes" }
  | { vista: "solicitud"; id: number }
  | { vista: "usuarios" }
  | { vista: "comision" }
  | { vista: "requisitos" }
  | { vista: "operacion" }
  | { vista: "inicio" };

export function leerRuta(hash = window.location.hash): Ruta {
  const partes = hash.replace(/^#\/?/, "").split("/").filter(Boolean);
  switch (partes[0]) {
    case "expediente":
      return { vista: "expediente" };
    case "consulta":
      return { vista: "consulta" };
    case "usuarios":
      return { vista: "usuarios" };
    case "comision":
      return { vista: "comision" };
    case "requisitos":
      return { vista: "requisitos" };
    case "operacion":
      return { vista: "operacion" };
    case "solicitudes": {
      const id = Number(partes[1]);
      return Number.isInteger(id) && id > 0 ? { vista: "solicitud", id } : { vista: "solicitudes" };
    }
    default:
      return { vista: "inicio" };
  }
}

export function enlace(ruta: Ruta): string {
  switch (ruta.vista) {
    case "solicitud":
      return `#/solicitudes/${ruta.id}`;
    case "inicio":
      return "#/";
    default:
      return `#/${ruta.vista}`;
  }
}

export function navegar(ruta: Ruta) {
  window.location.hash = enlace(ruta);
}

export function useRuta(): Ruta {
  const [ruta, setRuta] = useState(leerRuta);
  useEffect(() => {
    const alCambiar = () => {
      setRuta(leerRuta());
      window.scrollTo(0, 0);
    };
    window.addEventListener("hashchange", alCambiar);
    return () => window.removeEventListener("hashchange", alCambiar);
  }, []);
  return ruta;
}
