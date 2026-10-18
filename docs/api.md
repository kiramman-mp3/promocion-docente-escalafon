# Referencia de la API

URL base en desarrollo: `http://localhost:5270`. La documentación interactiva está en **`/swagger`**: pulsa *Authorize* y pega el token de `/api/Auth/login`.

## Convenciones

- Cuerpos y respuestas en JSON con propiedades en `camelCase`.
- Autenticación con `Authorization: Bearer <token>`. El token dura 8 horas.
- Los errores de negocio responden `{ "mensaje": "texto para mostrar al usuario" }`.
- Las fechas que guarda el sistema (solicitudes y revisiones) van en UTC con `Z`; las fechas de las bases de origen son fechas sin zona horaria.
- **Cédula propia**: en los endpoints marcados así, un usuario con rol Docente solo puede usar su propia cédula; con otra recibe 403.

| Código | Significado |
|--------|-------------|
| 200 / 201 / 204 | Correcto / creado / sin contenido |
| 400 | Datos inválidos (falta un motivo, cédula mal escrita…) |
| 401 | Sin token, token vencido o credenciales inválidas |
| 403 | El rol no tiene permiso o la cédula no es la propia |
| 404 | No existe (cédula sin registro en RRHH, solicitud inexistente…) |
| 409 | Conflicto con el estado actual (solicitud pendiente duplicada, ya resuelta…) |

## Autenticación

### `POST /api/Auth/login` · público

```json
{ "correo": "ana.perez@uta.edu.ec", "contrasena": "Uta2026*" }
```

```json
{
  "token": "eyJhbGciOi…",
  "id": 3,
  "nombreCompleto": "Ana María Pérez Villacís",
  "correo": "ana.perez@uta.edu.ec",
  "rol": "Docente",
  "cedula": "1801234567"
}
```

401 si el correo no existe, la contraseña no coincide o el usuario está desactivado.

## Catálogo del escalafón

### `GET /api/roles` · público

```json
[{ "id": 1, "nombre": "Auxiliar 1", "requiereAniosEnRol": 0, "requiereObras": 0, "requiereEvaluacion": 0.00,
   "requiereCapacitacionHoras": 0, "requiereInvestigacionMeses": 0 }, …]
```

## Importación del expediente

Todos son `POST`, requieren token y aplican **cédula propia**. Cuerpo: `{ "cedula": "1801234567" }`, salvo donde se indica.

| Endpoint | Origen | Respuesta |
|----------|--------|-----------|
| `/api/importar/tiempo-rol` | RRHH | `{ id, cedula, rolActual, aniosEnRol, fechaInicio, pdfBase64 }` · 404 si la cédula no está en RRHH |
| `/api/importar/obras` | DIDE | Cuerpo `{ cedula, fechaInicio }`. Lista de `{ id, cedula, fechaPublicacion, nombreRevista, pdfBase64 }` publicadas desde `fechaInicio` · 404 si no hay |
| `/api/importar/cursos` | DEaDV | Lista de `{ id, cedula, universidad, facultad, fechaEvaluacion, puntaje }` · 404 si no hay |
| `/api/importar/investigaciones` | DIDE | Lista de `{ id, cedula, nombre, encargado, horas, pdf }` (`pdf` en base64) · 404 si no hay |
| `/api/importar/evaluaciones` | DEaDV | Lista de `{ id, cedula, periodo, universidad, carrera, puntaje, pdf }` · 404 si no hay |
| `/api/importar/tesis-doctorado` | DAC | `{ cedula, cantidad }` |
| `/api/importar/obras-idioma` | DIDE | `{ cedula, tieneObraEnOtroIdioma }` |
| `/api/importar/proyectos-ponderados` | DIDE | `{ cedula, mesesPonderados }` (coordinador ×2, subrogante ×1,5) |
| `/api/importar/capacitacion-impartida` | DEaDV | `{ cedula, horasImpartidas }` |

> Los endpoints de listas responden **404 cuando no hay registros**. El frontend lo trata como una lista vacía.

## Validación de la promoción

### `POST /api/promocion/validar` · token · cédula propia

```json
{ "cedula": "1801234567", "rolActualId": 3 }
```

```json
{
  "rolObjetivo": "Agregado 2",
  "cumpleExperiencia": true,
  "cumpleObras": true,
  "cumpleEvaluacion": true,
  "cumpleCapacitacion": true,
  "cumpleInvestigacion": true,
  "cumpleTesisDoctorado": true,
  "observaciones": ""
}
```

Responde 500 si la cédula no está en RRHH o si `rolActualId` ya es el último rol. Las reglas de cálculo tienen limitaciones documentadas en [problemas conocidos](problemas-conocidos.md).

## Solicitudes de promoción

| Método y ruta | Permiso | Descripción |
|---------------|---------|-------------|
| `POST /api/solicitudes` | token · cédula propia | Crea una solicitud. Cuerpo `{ "cedula": "…" }`. Responde 201 con la solicitud. |
| `GET /api/solicitudes?estado=Pendiente` | token | Docente: sus solicitudes. RRHH/Administrador: todas. `estado` es opcional (`Pendiente`, `Aprobada`, `Rechazada`). Las pendientes salen primero. |
| `GET /api/solicitudes/{id}` | token | Detalle con sus revisiones. 403 si un docente pide una ajena. |
| `PUT /api/solicitudes/{id}/documentos` | RRHH, Administrador | Aprueba o rechaza un documento. |
| `PUT /api/solicitudes/{id}/resolucion` | RRHH, Administrador | Aprueba o rechaza la solicitud. |

Revisar un documento:

```json
{ "tipoDocumento": "Obra", "documentoId": 5, "estado": "Rechazado", "observacion": "La revista no está indexada" }
```

`tipoDocumento`: `TiempoRol`, `Obra`, `Evaluacion`, `Investigacion` o `Curso`. `documentoId` es el `id` que devuelve el endpoint de importación. `observacion` es obligatoria al rechazar.

Resolver:

```json
{ "estado": "Rechazada", "observacion": "Debe completar 4 evaluaciones docentes." }
```

Respuesta de una solicitud:

```json
{
  "id": 2,
  "cedula": "1809876543",
  "rolActualId": 2,
  "rolActual": "Auxiliar 2",
  "rolObjetivo": "Agregado 1",
  "estado": "Rechazada",
  "solicitadoPor": "Carlos Andrés Gómez Lalama",
  "fechaSolicitud": "2026-10-18T03:14:27.05Z",
  "resueltoPor": "Paola Sánchez",
  "fechaResolucion": "2026-10-18T03:20:11.70Z",
  "observacion": "Debe completar 4 evaluaciones docentes.",
  "revisiones": [
    { "tipoDocumento": "Obra", "documentoId": 5, "estado": "Rechazado", "observacion": "La revista no está indexada",
      "revisadoPor": "Paola Sánchez", "fechaRevision": "2026-10-18T03:18:02.11Z" }
  ]
}
```

En el listado `revisiones` es `null`. Las reglas y sus códigos de error están en [roles y flujo](roles-y-flujo.md#reglas).

## Usuarios · solo Administrador

| Método y ruta | Descripción |
|---------------|-------------|
| `GET /api/usuarios` | Lista de usuarios, sin contraseñas |
| `POST /api/usuarios` | Crea un usuario. 201, 400 o 409 (correo repetido) |
| `PUT /api/usuarios/{id}` | Cambia nombre, apellido, rol, cédula y si está activo |
| `PUT /api/usuarios/{id}/contrasena` | Restablece la contraseña. 204 |

```json
{
  "correo": "maria.lopez@uta.edu.ec",
  "nombre": "María",
  "apellido": "López",
  "rol": "Docente",
  "cedula": "1700000001",
  "contrasena": "Temporal2026"
}
```

Validaciones: correo con formato válido (se guarda en minúsculas), contraseña de 8 caracteres o más, rol `Docente`, `RRHH` o `Administrador`, cédula de 10 dígitos y obligatoria para docentes.

## Probar la API a mano

`ApiREST_UTA/ApiREST_UTA.http` tiene peticiones listas para Visual Studio o la extensión REST Client de VS Code. `tests/e2e/api-smoke.sh` recorre todos los flujos con `curl`.
