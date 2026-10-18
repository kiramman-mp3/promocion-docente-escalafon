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
├── secciones.ts          Secciones de cada rol y rutaPermitida()
├── requisitos.ts         Texto de cada requisito del dictamen a partir del expediente
├── useExpediente.ts      Carga el expediente de una cédula; cumpleTodo()
├── format.ts             Fechas y números en es-EC, plural()
├── styles.css            Estilos (tokens de color y tipografía en :root)
├── components/
│   ├── Login.tsx              Pantalla de ingreso
│   ├── Cabecera.tsx           Navegación según el rol
│   ├── Escalafon.tsx          La escalera de roles (rol actual, siguiente, "puede ascender")
│   ├── Dictamen.tsx           Veredicto y lista de requisitos
│   ├── Evidencia.tsx          Pestañas con los documentos; en modo revisión agrega la columna "Revisión"
│   ├── ControlRevision.tsx    Aprobar / rechazar un documento (con motivo)
│   ├── PanelSolicitud.tsx     Solicitar promoción o ver la solicitud en curso
│   └── EtiquetaEstado.tsx     Etiqueta Pendiente / Aprobada / Rechazada
├── vistas/
│   ├── MiExpediente.tsx       Docente
│   ├── Consulta.tsx           RRHH y Administrador: buscar por cédula
│   ├── Solicitudes.tsx        Bandeja con filtros por estado
│   ├── SolicitudDetalle.tsx   Revisión de documentos y resolución
│   └── Usuarios.tsx           Administración de cuentas
└── test/                     Configuración y datos de las pruebas
```

## Rutas

| Hash | Vista | Roles |
|------|-------|-------|
| `#/expediente` | Mi expediente | Docente |
| `#/consulta` | Consultar docente | RRHH, Administrador |
| `#/solicitudes` | Bandeja o mis solicitudes | Todos |
| `#/solicitudes/{id}` | Detalle de la solicitud | Todos (la API valida que un docente solo vea las suyas) |
| `#/usuarios` | Usuarios | Administrador |

Una ruta no permitida redirige a la primera sección del rol (`secciones.ts`).

## Sesión

- El login devuelve el token y los datos del usuario; se guardan en `sessionStorage`, así que la sesión se cierra al cerrar la pestaña.
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

Accesibilidad: foco visible, pestañas con el patrón ARIA (flechas izquierda/derecha), etiquetas para lectores de pantalla en los botones de cada documento, `prefers-reduced-motion` respetado y diseño probado a 390 px de ancho.

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
