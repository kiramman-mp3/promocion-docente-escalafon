# Frontend

Aplicación de una sola página en React 18 + TypeScript, construida con Vite 5. No usa librerías de componentes ni de rutas: todo está en `frontend/src/`.

## Estructura

```
src/
├── main.tsx              Punto de entrada; carga fuentes (Public Sans, Literata) y estilos
├── App.tsx               Sesión, catálogo de roles, protección de rutas y elección de vista
├── api.ts                Cliente HTTP de la API (un función por endpoint) y ApiError
├── types.ts              Tipos de las respuestas de la API
├── sesion.ts             Contexto de sesión y hook useSesion()
├── ruta.ts               Rutas por hash: leerRuta(), enlace(), navegar(), useRuta()
├── secciones.ts          Secciones de cada usuario (rol y Comisión) y rutaPermitida()
├── useExpediente.ts      Carga el expediente y el dictamen de una cédula; cumpleTodo()
├── format.ts             Fechas y números en es-EC, plural() y nombres de estados, cargos y tipos
├── styles.css            Estilos (tokens de color y tipografía en :root)
├── components/
│   ├── Login.tsx              Pantalla de ingreso
│   ├── Cabecera.tsx           Navegación según el rol y la Comisión
│   ├── Escalafon.tsx          La escalera de grados (actual, siguiente, "por concurso")
│   ├── Dictamen.tsx           Veredicto y requisitos con artículo, obtenido, exigido y excepciones
│   ├── Evidencia.tsx          Pestañas con los documentos de las bases institucionales
│   ├── ControlRevision.tsx    Aprobar / rechazar un documento (con motivo)
│   ├── PanelSolicitud.tsx     Presentar la solicitud (art. 99) o ver la que está en curso
│   ├── EtiquetaEstado.tsx     Etiqueta del estado de la solicitud
│   └── solicitud/
│       ├── Evidencias.tsx         Anexar y descargar evidencias en PDF
│       ├── RevisionDocumental.tsx Documentos agrupados por tipo y envío a la Comisión
│       ├── Votacion.tsx           Conteo, votos motivados, voto propio y cierre
│       └── Resolucion.tsx         Acto en PDF, acción de personal y apelación
├── vistas/
│   ├── MiExpediente.tsx       Docente
│   ├── Consulta.tsx           Talento Humano, Administrador y Comisión: buscar por cédula
│   ├── Solicitudes.tsx        Bandeja con filtros por estado
│   ├── SolicitudDetalle.tsx   Línea de tiempo del trámite y todas sus etapas
│   ├── Comision.tsx           Solicitudes en votación e integrantes de la Comisión
│   ├── Requisitos.tsx         Versiones de los requisitos de promoción
│   ├── Operacion.tsx          Disponibilidad, reporte de uso y bitácora de auditoría
│   └── Usuarios.tsx           Administración de cuentas
└── test/                     Configuración y datos de las pruebas
```

## Rutas

| Hash | Vista | Roles |
|------|-------|-------|
| `#/expediente` | Mi expediente | Docente |
| `#/consulta` | Consultar docente | RRHH, Administrador, miembros de la Comisión |
| `#/solicitudes` | Bandeja o mis solicitudes | Todos |
| `#/solicitudes/{id}` | Detalle de la solicitud | Todos (la API valida quién puede ver cada una) |
| `#/comision` | Comisión Académica | Miembros de la Comisión, Administrador |
| `#/usuarios` | Usuarios | Administrador |
| `#/requisitos` | Requisitos de promoción | Administrador |
| `#/operacion` | Operación | Administrador |

Una ruta no permitida redirige a la primera sección del usuario (`secciones.ts`). Qué ve cada rol está en [roles y flujo](roles-y-flujo.md#secciones-del-frontend).

## Sesión

- El login devuelve el token, los datos del usuario y su pertenencia a la Comisión; se guardan en `sessionStorage`, así que la sesión se cierra al cerrar la pestaña.
- `useSesion()` da acceso a la sesión, al catálogo de roles, a `esRevisor` y a `mensajeDeError(e)`, que cierra la sesión si la API responde 401.
- El catálogo de roles (`GET /api/roles`) se carga al abrir la aplicación, antes del login.

## Configuración

| Variable | Uso |
|----------|-----|
| `VITE_API_URL` | URL de la API. Se fija **al compilar**. Por defecto `http://localhost:5270`. |

En `npm run dev`, las ayudas con usuarios y cédulas de prueba solo aparecen en modo desarrollo (`import.meta.env.DEV`); no se incluyen en el build de producción.

## Diseño

| Token | Valor | Uso |
|-------|-------|-----|
| `--papel` | `#f6f7f5` | Fondo |
| `--tinta` | `#1d2633` | Texto |
| `--granate` | `#7a1f2b` | Acciones principales, rol actual |
| `--verde` | `#2e6b4e` | Cumple, aprobado |
| `--oxido` | `#b04a1e` | No cumple, rechazado |
| `--linea` | `#d8dce1` | Bordes |

Tipografías: Literata (títulos y cifras) y Public Sans (interfaz), servidas desde el propio bundle con `@fontsource`.

Accesibilidad: foco visible, pestañas con el patrón ARIA (flechas izquierda/derecha), etiquetas para lectores de pantalla en los botones de cada documento, `prefers-reduced-motion` respetado y diseño probado a 390 px de ancho (las tablas anchas y las pestañas se desplazan dentro de su contenedor, no la página).

## Scripts

| Comando | Qué hace |
|---------|----------|
| `npm run dev` | Servidor de desarrollo en http://localhost:5173 |
| `npm run build` | Verifica tipos y genera `dist/` |
| `npm run lint` | ESLint sin advertencias permitidas |
| `npm run typecheck` | TypeScript sin emitir archivos |
| `npm test` | Pruebas con Vitest |
| `npm run test:coverage` | Pruebas con informe de cobertura en `coverage/` |

En Docker, `frontend/Dockerfile` compila con Node 20 y sirve `dist/` con nginx en el puerto 8080.
