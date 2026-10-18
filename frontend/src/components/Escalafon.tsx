import { useEffect, useState } from "react";
import { ESCALAFON } from "../roles";

interface Props {
  rolActualId?: number;
  // true cuando el dictamen confirma que cumple todos los requisitos
  alcanzaObjetivo?: boolean;
}

const ALTO_PELDANO = 56;

export function Escalafon({ rolActualId, alcanzaObjetivo = false }: Props) {
  const objetivoId = rolActualId && rolActualId < ESCALAFON.length ? rolActualId + 1 : undefined;

  // El riel se rellena hasta el rol actual y, si cumple, sube al objetivo después de un instante.
  const [subido, setSubido] = useState(false);
  useEffect(() => {
    setSubido(false);
    if (!alcanzaObjetivo) return;
    const t = setTimeout(() => setSubido(true), 350);
    return () => clearTimeout(t);
  }, [rolActualId, alcanzaObjetivo]);

  const nivelRelleno = rolActualId ? (subido && objetivoId ? objetivoId : rolActualId) : 0;
  const altoRelleno = nivelRelleno ? (nivelRelleno - 1) * ALTO_PELDANO : 0;
  const peldanos = [...ESCALAFON].reverse();

  return (
    <nav className="escalafon" aria-label="Escalafón docente">
      <h2 className="escalafon__titulo">Escalafón</h2>
      <ol className="escalafon__lista" style={{ ["--alto-peldano" as string]: `${ALTO_PELDANO}px` }}>
        <span className="escalafon__riel" aria-hidden="true">
          <span className="escalafon__relleno" style={{ height: altoRelleno }} />
        </span>
        {peldanos.map((rol) => {
          const estado =
            rol.id === rolActualId
              ? "actual"
              : rol.id === objetivoId
                ? subido
                  ? "alcanzado"
                  : "objetivo"
                : rolActualId && rol.id < rolActualId
                  ? "recorrido"
                  : "pendiente";
          return (
            <li
              key={rol.id}
              className={`peldano peldano--${estado}`}
              aria-current={estado === "actual" ? "step" : undefined}
            >
              <span className="peldano__punto" aria-hidden="true" />
              <span className="peldano__nombre">{rol.nombre}</span>
              {estado === "actual" && <span className="peldano__nota">Rol actual</span>}
              {(estado === "objetivo" || estado === "alcanzado") && (
                <span className="peldano__nota">{estado === "alcanzado" ? "Puede ascender" : "Siguiente rol"}</span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
