# Plan de pruebas

Exigido por el Reglamento de la DITIC (arts. 23 f y 27). Define qué se prueba, cómo, con qué criterio de aceptación y dónde queda la evidencia. El detalle de cómo ejecutar cada nivel está en [pruebas y CI](pruebas.md).

## Alcance

| Nivel | Qué verifica | Herramienta | Cuándo | Criterio de aceptación | Evidencia |
|-------|--------------|-------------|--------|------------------------|-----------|
| Reglas normativas | Cada requisito del CES y de la UTA con casos que cumplen, que no cumplen y límites | xUnit (`Promocion/ReglasPromocionTests`) | Cada push | 100 % aprobadas | Artefacto `resultados-api` |
| Servicios | Procedimiento: solicitud, revisión, votación, acto, apelación, Comisión, evidencias, usuarios, login | xUnit + Moq | Cada push | 100 % aprobadas | `resultados-api` |
| Seguridad unitaria | Cédula propia, permisos por claims, enmascarado de secretos en la auditoría | xUnit | Cada push | 100 % aprobadas | `resultados-api` |
| Frontend | Rutas y permisos por rol, cliente HTTP, componentes de login, escalafón y revisión | Vitest + Testing Library | Cada push | 100 % aprobadas, lint y tipos sin errores | `cobertura-frontend` |
| Punta a punta | Trámite completo sobre el stack real con datos de prueba | `tests/e2e/api-smoke.sh` | Cada push | Todas las comprobaciones correctas | Log del job `e2e` |
| Respaldo | Respaldo de las 5 bases verificado con `RESTORE VERIFYONLY` | `db-backup` | Cada push | Sin errores | Log del job `e2e` |
| Restauración | Restaurar una base desde su último respaldo y que la API siga respondiendo | `db-restore` + `GET /health` | Mensual y antes de cada despliegue | `Healthy` y datos consultables | Registro manual |
| Seguridad dinámica | Vulnerabilidades comunes del frontend y la API | OWASP ZAP baseline y API scan | Cada push | Sin alertas altas sin justificar | Artefacto `seguridad-zap` |
| Rendimiento | 25 usuarios concurrentes consultando dictamen, obras, roles y solicitudes | k6 (`tests/performance/carga.js`) | Cada lunes y a mano | Errores < 1 %, p95 < 800 ms, dictamen p95 < 1,5 s | Log del workflow *Rendimiento* |
| Calidad de código | Formato y reglas de estilo | `dotnet format`, ESLint, TypeScript estricto | Cada push | Sin cambios pendientes ni advertencias | Log del job |
| Revisión visual | Pantallas de cada rol en escritorio y móvil (390 px) sin desbordes | Navegador | Antes de cada versión | Sin scroll horizontal ni errores de consola | Capturas |

## Escenarios de punta a punta

Con los datos de prueba de [base de datos](base-de-datos.md#datos-de-prueba):

| # | Escenario | Resultado esperado |
|---|-----------|--------------------|
| E1 | Ana (cumple todo) consulta su dictamen | `cumpleTodo = true` hacia Agregado 2 |
| E2 | Marta cumple solo con las excepciones (transitoria quinta, art. 77 f) | `cumpleTodo = true` |
| E3 | Carlos solicita sin la permanencia mínima | 400 (art. 99) |
| E4 | Jorge solicita desde Agregado 3 | 400 (art. 76) |
| E5 | Ana sube un PDF real, un archivo falso y otro docente intenta subir | 201, 400, 403 |
| E6 | Talento Humano intenta enviar con documentos sin revisar, revisa todo y envía | 409, luego `EnComision` |
| E7 | Votación: no miembro, certificación por quien no es DF, cierre sin quórum, cierre por quien no preside, cierre correcto | 403, 400, 409, 403, `Aprobada` |
| E8 | Descarga del acto por la docente y por un tercero | PDF, 403 |
| E9 | Acción de personal con fecha futura y válida | 400, `Ejecutada` |
| E10 | Lucía (miembro) vota su propia solicitud | 403 |
| E11 | Negativa, apelación de un tercero, apelación de Lucía, resolución del HCU por RRHH y por el Administrador, segunda apelación | `Rechazada`, 403, `Apelada`, 403, `Aprobada`, 409 |
| E12 | Requisitos bajo el mínimo del CES, auditoría, reporte de uso, usuarios | 400, registros presentes, 200, alta y bloqueo |

## Última ejecución

18 de octubre de 2026, rama `feature/cumplimiento-normativo`, sobre un stack recién creado:

| Nivel | Resultado |
|-------|-----------|
| xUnit | 125 de 125 aprobadas |
| Vitest | 31 de 31 aprobadas |
| `dotnet format`, ESLint, TypeScript, build de Vite | Sin errores |
| Punta a punta | 86 de 86 comprobaciones correctas |
| Respaldo y restauración | Respaldo de las 5 bases verificado; DAC restaurada y la API respondió `Healthy` |
| k6 | 3 821 peticiones, 0 % de errores, p95 61 ms (dictamen 74 ms) |
| Límite de login | Con límite 3: tres 401 y luego 429 |
| Revisión visual | Sin errores de consola; el desborde horizontal en móvil se corrigió |

## Defectos encontrados al verificar

| Defecto | Corrección |
|---------|------------|
| Swagger respondía 500 por la subida de evidencias (`IFormFile` con parámetros sueltos) y el escaneo de ZAP de la API no podía ejecutarse | Modelo `SubirEvidenciaRequest` con `[FromForm]` |
| El login no devolvía la pertenencia a la Comisión, así que el frontend nunca mostraba la sección | `AuthService` devuelve `esMiembroComision`, `esPresidenteComision` y `cargoComision` |
| Una autoridad fuera de la Comisión aterrizaba en una vista no permitida (redirección en bucle) y veía *Consultar docente*, que la API le niega | Secciones alineadas con los permisos de la API |
| En móvil, las cabeceras solo para lectores de pantalla de las tablas ensanchaban la página | Contenedores de tablas con `position: relative` |
