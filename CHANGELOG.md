# Changelog

Todos los cambios relevantes del proyecto se documentan aquí.
El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa [versionado semántico](https://semver.org/lang/es/).

## [Sin publicar]

### Agregado
- **Roles y flujo de aprobación**: login común para Docente, RRHH y Administrador; solicitudes de promoción; revisión de cada documento del expediente con motivo de rechazo; resolución de la solicitud.
- **Gestión de usuarios** para administradores: alta, edición de rol y cédula, activación y restablecimiento de contraseña.
- Endpoints `GET /api/roles`, `/api/solicitudes` y `/api/usuarios`; los endpoints de importación devuelven el `id` de cada documento.
- Tablas `SolicitudesPromocion` y `RevisionesDocumento` en DITIC y columna `Usuarios.Activo`.
- **Frontend** en React + Vite + TypeScript: expediente con dictamen, escalera del escalafón, documentos con visor de PDF, bandeja de solicitudes, revisión y administración de usuarios.
- **Docker Compose** con SQL Server 2022, inicialización automática de las 5 bases, API y frontend con nginx.
- Scripts SQL de esquema, login de permisos mínimos y datos de prueba.
- Configuración por `.env` (cadenas de conexión, clave JWT, CORS) con `.env.example` documentado.
- Swagger con autorización por JWT y CORS configurable.
- Pruebas: 46 unitarias de la API (xUnit + Moq), 32 del frontend (Vitest + Testing Library) y una prueba de punta a punta contra Docker Compose.
- Linters: `dotnet format` con `.editorconfig`, ESLint y TypeScript estricto.
- GitHub Actions (lint, pruebas, build y punta a punta) y Dependabot.
- Documentación en `docs/`, licencia MIT, guía de contribución y política de seguridad.

### Cambiado
- Un docente solo puede consultar su propia cédula (filtro `[SoloCedulaPropia]`).
- El login incluye el id y la cédula del usuario, y rechaza usuarios desactivados.
- La API usa un login de base de datos con permisos mínimos en lugar de una conexión de confianza.

### Corregido
- Las contraseñas se comparaban en texto plano; ahora se verifican con BCrypt.
- El token se firmaba con una clave escrita en el código en vez de la configurada.
- La cadena de conexión se imprimía en la consola.
- `bin/`, `obj/` y `.vs/` estaban versionados; se agregó un `.gitignore`.
- La carpeta `Front` era un submódulo roto sin contenido.

### Seguridad
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
