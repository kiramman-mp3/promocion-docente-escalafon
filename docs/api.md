# Referencia de la API

URL base en desarrollo: `http://localhost:5270`. La documentación interactiva está en **`/swagger`**: pulsa *Authorize* y pega el token de `/api/Auth/login`.

## Convenciones

- Cuerpos y respuestas en JSON con propiedades en `camelCase`.
- Autenticación con `Authorization: Bearer <token>`. El token dura 8 horas, pero un usuario desactivado pierde el acceso en su siguiente petición.
- Los errores de negocio responden `{ "mensaje": "texto para mostrar al usuario" }`.
- Las fechas que guarda el sistema (solicitudes, revisiones, votos, auditoría) van en UTC con `Z`; las fechas de las bases de origen son fechas sin zona horaria.
- **Cédula propia**: en los endpoints marcados así, un usuario con rol Docente solo puede usar su propia cédula; con otra recibe 403. Los miembros de la Comisión Académica pueden consultar cualquier cédula.
- Toda operación que cambia datos queda en la [bitácora de auditoría](#operación--solo-administrador).

| Código | Significado |
|--------|-------------|
| 200 / 201 / 204 | Correcto / creado / sin contenido |
| 400 | Datos inválidos (falta un motivo, no cumple la permanencia, archivo que no es PDF…) |
| 401 | Sin token, token vencido, usuario desactivado o credenciales inválidas |
| 403 | El rol no tiene permiso, la cédula no es la propia o hay conflicto de interés |
| 404 | No existe (cédula sin registro en RRHH, solicitud inexistente…) |
| 409 | Conflicto con el estado actual (solicitud en trámite, sin quórum, plazo vencido…) |
| 429 | Demasiados intentos de login desde la misma IP (`LOGIN_RATE_LIMIT` por minuto) |

## Autenticación

### `POST /api/Auth/login` · público · limitado por IP

```json
{ "correo": "lucia.mora@uta.edu.ec", "contrasena": "Uta2026*" }
```

```json
{
  "token": "eyJhbGciOi…",
  "id": 5,
  "nombreCompleto": "Lucía Fernanda Mora Andrade",
  "correo": "lucia.mora@uta.edu.ec",
  "rol": "Docente",
  "cedula": "1712345678",
  "esMiembroComision": true,
  "esPresidenteComision": false,
  "cargoComision": "ProfesorTerna"
}
```

401 si el correo no existe, la contraseña no coincide o el usuario está desactivado. El token lleva el claim `comision=true` cuando el usuario integra la Comisión Académica.

## Catálogos

### `GET /api/roles` · público

Escalafón del CES (art. 63). `promocionable` es `false` en Agregado 3 y Principal 3: desde ahí se cambia de categoría por concurso (art. 76).

```json
[{ "id": 3, "nombre": "Agregado 1", "categoria": "Agregado", "nivel": 1, "grado": 3, "permanenciaAnios": 4, "promocionable": true }, …]
```

### `GET /api/requisitos?historico=false` · público

Requisitos de promoción **versionados** por grado de origen. Con `historico=true` incluye las versiones que ya no rigen.

```json
{
  "id": 3, "gradoOrigen": 3, "rolOrigen": "Agregado 1", "gradoDestino": 4, "rolDestino": "Agregado 2",
  "normativa": "CES RPC-SE-19-No.055-2021, arts. 76 y 77; UTA 0153-HCU-2026, art. 48",
  "resolucion": "Propuesta pendiente de la normativa interna de promoción (UTA, transitoria cuarta)",
  "esPropuesta": true, "vigenteDesde": "2026-03-26T00:00:00", "vigenteHasta": null, "vigente": true,
  "obrasTotales": 3, "obrasEnGrado": 1, "evaluacionMinima": 75, "evaluacionPeriodos": 4,
  "capacitacionHoras": 96, "capacitacionAnios": 3, "capacitacionPctPedagogica": 25,
  "proyectosMeses": 12, "requiereDireccionProyecto": false, "trabajosTitulacion": 1, "titulacionNivelMinimo": "Grado"
}
```

### `POST /api/requisitos` · Administrador

Crea una nueva versión para un grado; la anterior queda con `vigenteHasta` el día previo. Nunca se edita una versión, porque las solicitudes ya presentadas se evalúan con la que regía al presentarlas (CES, transitoria novena). Cuerpo: los mismos campos de la respuesta, sin `id`, `rol*`, `gradoDestino`, `vigenteHasta` ni `vigente`.

Responde 400 si el grado no tiene promoción; si algún valor queda por debajo del mínimo del CES (art. 76): al menos 1 obra en el grado, evaluación ≥ 75 % en las 4 últimas evaluaciones, 96 horas en 3 años con 25 % pedagógico; si los requisitos dejan de ser crecientes respecto del grado anterior o el siguiente; o si `vigenteDesde` no es posterior a la versión vigente.

## Importación del expediente

Todos son `POST`, requieren token y aplican **cédula propia**. Cuerpo: `{ "cedula": "1801234567" }`. Las listas vacías responden `200 []`.

| Endpoint | Origen | Respuesta |
|----------|--------|-----------|
| `/api/importar/tiempo-rol` | RRHH | `{ id, cedula, rolActual, aniosEnRol, fechaInicio, pdfBase64 }` · 404 si la cédula no está en RRHH |
| `/api/importar/trayectoria` | RRHH | `{ trayectoria, cargosAutoridad[], sabaticos[] }`: ingreso a la institución y a la titularidad, cargos de autoridad y periodos sabáticos |
| `/api/importar/obras` | DIDE | Obras con `tipoObra`, `issn`, `isbn`, `baseIndexacion`, `revisionPares`, `registroSENADI`, `esRelevante` y `motivoNoRelevante` (CES arts. 78-87). Cuerpo opcional `fechaInicio` |
| `/api/importar/investigaciones` | DIDE | `{ id, cedula, nombre, encargado, horas, pdf }` |
| `/api/importar/proyectos` | DIDE | `{ id, cedula, nombreProyecto, rol, tipoProyecto, meses, mesesPonderados, fechaInicio, fechaFin }` (`Director`, `Codirector`, `Participante`) |
| `/api/importar/cursos` | DEaDV | `{ id, nombreCurso, horas, tipoCapacitacion, fechaInicio, fechaFin, … }` (`Pedagogica`, `Cientifica`, `CongresoInternacional`) |
| `/api/importar/evaluaciones` | DEaDV | `{ id, periodo, periodoAcademico, puntaje, ejercioDocencia, pdf, … }` |
| `/api/importar/equivalencias` | DEaDV | `{ id, cedula, tipo, fecha, meses, descripcion, horasCapacitacion }`: evaluador externo, facilitador CES, par evaluador CACES (CES art. 77 g-i) |
| `/api/importar/trabajos-titulacion` | DAC | `{ id, titulo, nivel, fechaAprobacion, … }` (`Grado`, `Maestria`, `Doctorado`) |
| `/api/importar/tesis-doctorado` | DAC | `{ cedula, cantidad }` de trabajos de doctorado |
| `/api/importar/obras-idioma` | DIDE | `{ cedula, tieneObraEnOtroIdioma }` |
| `/api/importar/proyectos-ponderados` | DIDE | `{ cedula, mesesPonderados }` (dirección ×2, codirección ×1,5; CES art. 36 c.2) |
| `/api/importar/capacitacion-impartida` | DEaDV | `{ cedula, horasImpartidas }` |

## Dictamen normativo

### `POST /api/promocion/validar` · token · cédula propia

```json
{ "cedula": "1712345678" }
```

Evalúa el expediente contra los requisitos vigentes del grado actual. Cada requisito trae el artículo que lo exige, lo obtenido, lo requerido y si se cumplió por una excepción (`exento`).

```json
{
  "cedula": "1712345678",
  "fechaCorte": "2026-10-18T00:00:00+00:00",
  "rolActualId": 6, "rolActual": "Principal 1",
  "rolObjetivoId": 7, "rolObjetivo": "Principal 2",
  "promocionable": true,
  "requisitosId": 5,
  "normativa": "CES RPC-SE-19-No.055-2021, arts. 76 y 77; UTA 0153-HCU-2026, art. 48",
  "resolucion": "Propuesta pendiente de la normativa interna de promoción (UTA, transitoria cuarta)",
  "requisitosPropuestos": true,
  "cumpleTodo": true,
  "requisitos": [
    {
      "codigo": "R2", "nombre": "Tiempo en el grado actual", "articulo": "CES arts. 63, 76 a y 77 a; UTA art. 48 a",
      "cumple": true, "exento": false,
      "obtenido": "8 años y 9 meses como Principal 1", "requerido": "3 años", "detalle": "Desde el 15/01/2018."
    }
  ],
  "alertas": []
}
```

Códigos de requisito: `R0` sin requisitos vigentes, `R1` grado promocionable, `R2` permanencia, `R3` obras, `R4` evaluación, `R5` capacitación, `R6` proyectos, `R7` dirección de proyectos, `R8` trabajos de titulación. La correspondencia con cada artículo está en [cumplimiento normativo](cumplimiento-normativo.md).

`alertas` avisa situaciones que no bloquean la promoción, por ejemplo dos periodos consecutivos con evaluación menor a 70 % (UTA 0665-CU-P-2024, art. 15).

## Solicitudes de promoción

| Método y ruta | Permiso | Descripción |
|---------------|---------|-------------|
| `POST /api/solicitudes` | token · cédula propia | Presenta la solicitud (CES art. 99). Cuerpo `{ "cedula": "…" }`. 201. |
| `GET /api/solicitudes?estado=…` | token | Docente: las suyas. Comisión: además las enviadas a ella. RRHH/Administrador: todas. |
| `GET /api/solicitudes/{id}` | token | Detalle con dictamen, documentos, revisiones, votos, votación y apelación. |
| `PUT /api/solicitudes/{id}/documentos` | RRHH, Administrador | Aprueba o rechaza un documento del expediente. |
| `POST /api/solicitudes/{id}/enviar-comision` | RRHH, Administrador | Cierra la revisión documental y congela el dictamen. |
| `PUT /api/solicitudes/{id}/votos` | miembro de la Comisión | Registra o cambia su voto. |
| `POST /api/solicitudes/{id}/cerrar-votacion` | presidencia de la Comisión | Resuelve y emite el acto administrativo. |
| `GET /api/solicitudes/{id}/acto` | quien puede ver la solicitud | Descarga la resolución en PDF. |
| `PUT /api/solicitudes/{id}/accion-personal` | RRHH, Administrador | Registra la acción de personal: la solicitud pasa a `Ejecutada`. |
| `POST /api/solicitudes/{id}/apelacion` | el docente afectado | Apela una negativa ante el Consejo Universitario. |
| `PUT /api/solicitudes/{id}/apelacion/resolucion` | Administrador | Registra lo que resolvió el HCU. |

Estados: `RevisionDocumental` → `EnComision` → `Aprobada` → `Ejecutada`, o `EnComision` → `Rechazada` → `Apelada` → `Aprobada` / `Rechazada`. El flujo completo y sus reglas están en [roles y flujo](roles-y-flujo.md).

### Presentar

Responde 400 si el grado no tiene promoción (Agregado 3, Principal 3) o si todavía no cumple la permanencia mínima; 409 si ya hay una solicitud en trámite, si la misma promoción ya fue aprobada o si no hay requisitos vigentes. Al crearla se guarda qué versión de requisitos rige (`requisitosPromocionId`).

### Revisar un documento

```json
{ "tipoDocumento": "Obra", "documentoId": 5, "estado": "Rechazado", "observacion": "La revista no está indexada" }
```

`tipoDocumento`: `TiempoRol`, `Obra`, `Evaluacion`, `Investigacion`, `Curso`, `Proyecto`, `TrabajoTitulacion`, `Equivalencia` o `Evidencia`. El documento debe pertenecer al expediente (400 si no). `observacion` es obligatoria al rechazar. La lista exacta de documentos a revisar está en `documentos` del detalle.

`enviar-comision` responde 409 mientras quede algún documento sin revisar.

### Votar y cerrar

```json
{ "voto": "AFavor", "motivacion": "Cumple los requisitos del art. 76", "certificaPresupuesto": true }
```

- `voto`: `AFavor`, `EnContra` o `Abstencion`. La motivación es obligatoria.
- `certificaPresupuesto` solo lo envía el Director Financiero (400 para otro cargo).
- 403 si quien vota no integra la Comisión o si vota su propia solicitud.

```json
{ "motivacion": "Cumple los requisitos del art. 76 del Reglamento de Carrera." }
```

`cerrar-votacion`: 403 si no preside; 409 sin quórum (4 votos) o si la mayoría aprueba sin certificación presupuestaria. Mayoría simple de votos emitidos; en empate decide el voto de la presidencia. Al cerrar se numera la resolución (`CAP-2026-0001`), se genera el PDF y se guarda su huella SHA-256 en `actoHash`.

### Acción de personal

```json
{ "numero": "AP-2026-0150", "fecha": "2026-10-18" }
```

400 si la fecha es futura o anterior a la resolución; 409 si la solicitud no está `Aprobada`.

### Apelación

```json
{ "fundamentos": "Las obras sí pertenecen a mi campo de conocimiento…" }
```

Dentro de 10 días hábiles desde la resolución (`plazoApelacionHasta`); 409 fuera de plazo o si ya se apeló. El Administrador registra la decisión del HCU:

```json
{ "aceptada": true, "resolucionHCU": "0200-HCU-2026", "motivacion": "El par externo certifica la pertinencia de las obras." }
```

Si se acepta, la solicitud queda `Aprobada` y se emite un nuevo acto; si se niega, vuelve a `Rechazada` y la decisión es definitiva.

### Respuesta de una solicitud (abreviada)

```json
{
  "id": 2, "cedula": "1712345678", "rolActualId": 6, "rolActual": "Principal 1", "rolObjetivo": "Principal 2",
  "estado": "Aprobada", "solicitadoPor": "Lucía Fernanda Mora Andrade",
  "fechaSolicitud": "2026-10-18T14:03:52.06Z", "requisitosPromocionId": 5,
  "fechaEnvioComision": "2026-10-18T14:04:06.24Z",
  "numeroResolucion": "CAP-2026-0002", "resueltoPor": "Administrador Sistema",
  "fechaResolucion": "2026-10-18T14:04:08.18Z",
  "observacion": "Apelación aceptada por el Consejo Universitario (resolución 0200-HCU-2026): …",
  "actoDisponible": true, "actoHash": "4d6342e8…",
  "accionPersonalNumero": null, "accionPersonalFecha": null, "fechaEjecucion": null,
  "dictamen": { "…": "el dictamen congelado al enviar a la Comisión" },
  "documentos": [{ "tipo": "TiempoRol", "id": 3, "descripcion": "Acción de personal: Principal 1 desde el 15/01/2018", "revision": { "estado": "Aprobado", "…": "…" } }],
  "votos": [{ "miembro": "Fernando Castillo Mena", "cargo": "VicerrectorAcademico", "esPresidente": true, "voto": "EnContra", "motivacion": "…", "certificaPresupuesto": null, "fecha": "…" }],
  "votacion": { "miembrosActivos": 6, "quorum": 4, "votosEmitidos": 4, "aFavor": 0, "enContra": 3, "abstenciones": 1,
                "presupuestoCertificado": false, "esMiembro": true, "puedeVotar": false, "motivoNoPuedeVotar": "La votación está cerrada.", "puedeCerrar": false },
  "apelacion": { "fundamentos": "…", "estado": "Aceptada", "resolucionHCU": "0200-HCU-2026", "motivacion": "…" },
  "plazoApelacionHasta": null,
  "puedeApelar": false
}
```

`votacion`, `puedeVotar`, `puedeCerrar` y `puedeApelar` se calculan para quien consulta. En el listado no se incluyen `documentos`, `votos` ni `dictamen`.

## Evidencias anexadas

Certificados que no están en las bases institucionales (congresos, equivalencias, sabático…).

| Método y ruta | Permiso | Descripción |
|---------------|---------|-------------|
| `GET /api/solicitudes/{id}/evidencias` | quien puede ver la solicitud | Lista sin el contenido |
| `POST /api/solicitudes/{id}/evidencias` | el docente de la solicitud, RRHH, Administrador | `multipart/form-data` con `archivo`, `tipo` y `descripcion`. 201 |
| `GET /api/solicitudes/{id}/evidencias/{evidenciaId}` | quien puede ver la solicitud | Descarga el PDF |

Solo PDF reales (se comprueba la cabecera `%PDF-`, no la extensión) de hasta 10 MB, y solo mientras la solicitud está en revisión documental (409 después). `tipo`: `CertificadoCongreso`, `Capacitacion`, `Equivalencia`, `Sabatico`, `CargoAutoridad`, `Obra`, `Proyecto` u `Otro`. Se guarda el hash SHA-256 de cada archivo.

## Comisión Académica

| Método y ruta | Permiso | Descripción |
|---------------|---------|-------------|
| `GET /api/comision/miembros?historico=false` | token | Integrantes vigentes (o todos) |
| `POST /api/comision/miembros` | Administrador | `{ usuarioId, cargo, esPresidente, vigenteDesde? }` |
| `POST /api/comision/miembros/{id}/finalizar` | Administrador | Termina la membresía hoy |

`cargo`: `VicerrectorAcademico` (único que puede presidir), `ProfesorHCU`, `ProfesorTerna` (dos), `DirectorFinanciero`, `DirectorTalentoHumano` (UTA art. 47). 409 si el cargo ya está cubierto o el usuario ya integra la Comisión.

## Usuarios · solo Administrador

| Método y ruta | Descripción |
|---------------|-------------|
| `GET /api/usuarios` | Lista de usuarios, sin contraseñas |
| `POST /api/usuarios` | Crea un usuario. 201, 400 o 409 (correo repetido) |
| `PUT /api/usuarios/{id}` | Cambia nombre, apellido, rol, cédula y si está activo |
| `PUT /api/usuarios/{id}/contrasena` | Restablece la contraseña. 204 |

```json
{ "correo": "maria.lopez@uta.edu.ec", "nombre": "María", "apellido": "López", "rol": "Docente", "cedula": "1700000001", "contrasena": "Temporal2026" }
```

Validaciones: correo con formato válido (se guarda en minúsculas), contraseña de 8 caracteres o más, rol `Docente`, `RRHH`, `Autoridad` o `Administrador`, cédula de 10 dígitos y obligatoria para docentes. Desactivar a un usuario invalida sus tokens de inmediato.

## Operación · solo Administrador

| Método y ruta | Descripción |
|---------------|-------------|
| `GET /api/auditoria?desde=&hasta=&usuarioId=&accion=&pagina=1&tamanoPagina=50` | Bitácora paginada: `{ total, pagina, tamanoPagina, registros[] }` |
| `GET /api/reportes/uso` | Ingresos de los últimos 30 días, operaciones, errores, usuarios activos, solicitudes por estado y días promedio hasta resolver |
| `GET /health` | **Público.** Estado de las cinco bases (`Healthy` 200 / `Unhealthy` 503) |

La bitácora registra cada operación que cambia datos y cada descarga de documentos: fecha, usuario, IP, `Controlador.Acción`, ruta, código de respuesta y los argumentos con contraseñas y tokens enmascarados (`***`). Las consultas no se registran.

```json
{ "estado": "Healthy", "duracionMs": 7, "componentes": { "sqlserver": { "estado": "Healthy", "descripcion": "Las cinco bases responden.",
  "bases": { "DITIC": "disponible", "RRHH": "disponible", "DIDE": "disponible", "DEaDV": "disponible", "DAC": "disponible" } } } }
```

## Probar la API a mano

`ApiREST_UTA/ApiREST_UTA.http` tiene peticiones listas para Visual Studio o la extensión REST Client de VS Code. `tests/e2e/api-smoke.sh` recorre el trámite completo con `curl`.
