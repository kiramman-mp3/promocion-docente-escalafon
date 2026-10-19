# Changelog

Todos los cambios relevantes del proyecto se documentan aquí.
El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa [versionado semántico](https://semver.org/lang/es/).

## [Sin publicar]

### Agregado
- **Cumplimiento normativo** (CES RPC-SE-19-No.055-2021, UTA 0153-HCU-2026, UTA 0665-CU-P-2024, Reglamento DITIC 0290-HCU-2026 y COA). Matriz en `docs/cumplimiento-normativo.md`.
- **Requisitos de promoción versionados** (`DITIC.RequisitosPromocion`, `GET/POST /api/requisitos`, pantalla *Requisitos*): validan los mínimos del CES y cada solicitud guarda la versión que la rige (transitoria novena).
- **Motor de reglas del dictamen** R1-R8 con el artículo, lo obtenido y lo exigido: grado con promoción, permanencia exacta, obras de relevancia (arts. 78-87), las 4 últimas evaluaciones con docencia, 96 horas en 3 años con 25 % pedagógico, proyectos, dirección de proyectos y trabajos de titulación; excepciones del art. 77 e-j y de la transitoria quinta; alerta de UTA-EV art. 15.
- **Datos de origen ampliados**: trayectoria, cargos de autoridad y sabáticos (RRHH); tipo y criterios de cada obra y proyectos con rol y fechas (DIDE); horas y tipo de capacitación, periodo y docencia de las evaluaciones, equivalencias (DEaDV); trabajos de titulación por nivel (DAC). Contrato en `docs/contrato-datos-ditic.md`.
- **Evidencias en PDF** anexadas a la solicitud (hasta 10 MB, validación del contenido, hash SHA-256).
- **Comisión Académica** (UTA art. 47): integrantes con cargo y vigencia, votación motivada con quórum, mayoría, voto dirimente, certificación presupuestaria y control de conflicto de interés.
- **Acto administrativo en PDF** numerado, con considerandos, verificación de requisitos, votos y huella SHA-256; registro de la **acción de personal**.
- **Apelación** ante el Consejo Universitario en 10 días hábiles (COA arts. 217-224).
- Rol de sistema **Autoridad**.
- **Operación**: bitácora de auditoría, reporte de uso, `/health`, respaldo y restauración verificados, prueba de carga con k6 y escaneo OWASP ZAP en CI.
- Frontend: dictamen por artículo, línea de tiempo del trámite, revisión documental, votación, resolución, apelación y pantallas de Comisión, Requisitos y Operación.
- Documentación: cumplimiento normativo, contrato de datos, plan de mejoramiento y plan de pruebas.
- **Roles y flujo de aprobación**: login común para Docente, RRHH y Administrador; solicitudes de promoción; revisión de cada documento del expediente con motivo de rechazo; resolución de la solicitud.
- **Gestión de usuarios** para administradores: alta, edición de rol y cédula, activación y restablecimiento de contraseña.
- Endpoints `GET /api/roles`, `/api/solicitudes` y `/api/usuarios`; los endpoints de importación devuelven el `id` de cada documento.
- Tablas `SolicitudesPromocion` y `RevisionesDocumento` en DITIC y columna `Usuarios.Activo`.
- **Frontend** en React + Vite + TypeScript: expediente con dictamen, escalera del escalafón, documentos con visor de PDF, bandeja de solicitudes, revisión y administración de usuarios.
- **Docker Compose** con SQL Server 2022, inicialización automática de las 5 bases, API y frontend con nginx.
- Scripts SQL de esquema, login de permisos mínimos y datos de prueba.
- Configuración por `.env` (cadenas de conexión, clave JWT, CORS) con `.env.example` documentado.
- Swagger con autorización por JWT y CORS configurable.
- Pruebas unitarias de la API (xUnit + Moq) y del frontend (Vitest + Testing Library), y una prueba de punta a punta contra Docker Compose.
- Linters: `dotnet format` con `.editorconfig`, ESLint y TypeScript estricto.
- GitHub Actions (lint, pruebas, build y punta a punta) y Dependabot.
- Documentación en `docs/`, licencia MIT, guía de contribución y política de seguridad.

### Cambiado
- Estados de la solicitud: `RevisionDocumental`, `EnComision`, `Aprobada`, `Rechazada`, `Apelada`, `Ejecutada` (las `Pendiente` se migran a `RevisionDocumental`). Ya no resuelve una sola persona de Talento Humano: decide la Comisión.
- `POST /api/promocion/validar` recibe solo la cédula y devuelve el dictamen normativo.
- Las listas de importación vacías responden `200 []` en vez de 404.
- `RRHH.Roles` describe el escalafón (categoría, nivel, grado, permanencia, si tiene promoción); los requisitos pasan a DITIC.
- `DAC.TesisDoctorado` se reemplaza por `TrabajosTitulacionDirigidos`.
- Pruebas: 125 de la API, 31 del frontend y 86 comprobaciones de punta a punta.
- Un docente solo puede consultar su propia cédula (filtro `[SoloCedulaPropia]`).
- El login incluye el id y la cédula del usuario, y rechaza usuarios desactivados.
- La API usa un login de base de datos con permisos mínimos en lugar de una conexión de confianza.

### Corregido
- Requisitos mal calculados: tesis doctoral que nunca se cumplía, puntajes sumados como horas, "20 horas = 1 mes", años en el rol por año calendario, promedio de todas las evaluaciones y promoción indebida de Agregado 3 a Principal 1.
- El dictamen respondía 500 en casos esperados.
- Se podía revisar un documento ajeno al expediente de la solicitud.
- Un usuario desactivado conservaba su token hasta 8 horas.
- Las contraseñas se comparaban en texto plano; ahora se verifican con BCrypt.
- El token se firmaba con una clave escrita en el código en vez de la configurada.
- La cadena de conexión se imprimía en la consola.
- `bin/`, `obj/` y `.vs/` estaban versionados; se agregó un `.gitignore`.
- La carpeta `Front` era un submódulo roto sin contenido.

### Seguridad
- Límite de intentos de login por IP (`LOGIN_RATE_LIMIT`) y revocación inmediata de usuarios desactivados.
- Bitácora de auditoría con contraseñas y tokens enmascarados; evidencias y auditoría solo admiten inserción.
- Se quitaron los secretos de `appsettings.json`. La clave JWT anterior sigue en el historial de git y no debe reutilizarse.

## [0.2.0] - Sprint 2

### Agregado
- Endpoints de tesis doctorales dirigidas, obras en idioma extranjero, meses ponderados en proyectos y capacitación impartida.
- Validación completa de la promoción (`POST /api/promocion/validar`).

## [0.1.0] - Sprint 1

### Agregado
- API REST en ASP.NET Core 8 con autenticación JWT.
- Importación del tiempo en el rol (RRHH), obras y investigaciones (DIDE), cursos y evaluaciones (DEaDV).

[Sin publicar]: https://github.com/kiramman-mp3/promocion-docente-escalafon/compare/main...develop
