# Guía de desarrollo

## Requisitos

| Herramienta | Versión | Para |
|-------------|---------|------|
| Docker Desktop | reciente | SQL Server y el stack completo |
| .NET SDK | 8.0 | Ejecutar y probar la API fuera de Docker |
| Node.js | 20 o superior | Frontend |
| Git | reciente | |

Sin el SDK de .NET instalado, todo puede hacerse con contenedores (ver más abajo).

## Puesta en marcha

### Todo en Docker

```bash
cp .env.example .env          # cambia contraseñas y JWT_KEY
docker compose up -d --build
```

Frontend en http://localhost:8080, API en http://localhost:5270 y Swagger en http://localhost:5270/swagger.

### API y frontend locales, base en Docker

```bash
docker compose up -d sqlserver db-init

cd ApiREST_UTA
dotnet run --launch-profile http       # lee el .env de la raíz

cd ../frontend
cp .env.example .env
npm install
npm run dev                            # http://localhost:5173
```

### Sin el SDK de .NET

```bash
docker run --rm -v "$PWD:/repo" -w /repo mcr.microsoft.com/dotnet/sdk:8.0 dotnet test PromocionDocente.sln
```

## Variables de entorno

Todas están en `.env` (ignorado por git). La plantilla documentada es `.env.example`.

| Variable | Obligatoria | Uso |
|----------|:-----------:|-----|
| `MSSQL_SA_PASSWORD` | ✔ | Contraseña de `sa` en el contenedor de SQL Server |
| `APP_DB_USER` / `APP_DB_PASSWORD` | ✔ | Login con permisos mínimos que usa la API |
| `JWT_KEY` | ✔ | Clave para firmar los tokens (32 caracteres o más) |
| `SQL_PORT` | | Puerto publicado de SQL Server (1433) |
| `DB_HOST` / `DB_PORT` | | Dónde busca la API a SQL Server con `dotnet run` (`localhost`, `SQL_PORT`) |
| `CORS_ORIGINS` | | Orígenes del frontend separados por coma |
| `SEED_DATA` | | `true` carga datos de prueba |
| `API_PORT` / `FRONTEND_PORT` | | Puertos publicados por Docker Compose (5270 / 8080) |
| `SWAGGER_ENABLED` | | `true` habilita Swagger fuera de `Development` |
| `ASPNETCORE_ENVIRONMENT` | | `Development` por defecto en Docker Compose |
| `LOGIN_RATE_LIMIT` | | Intentos de login por minuto y por IP (10). Súbelo para la prueba de punta a punta |
| `RETENCION_DIAS` | | Días que se conservan los respaldos de `db-backup` (30) |

SQL Server exige contraseñas de 8 caracteres o más con mayúsculas, minúsculas, números y símbolos. Evita comillas y espacios.

## Estructura del repositorio

```
ApiREST_UTA/            API .NET 8
frontend/               React + Vite
database/               Scripts SQL (schema, security, seed)
docker/                 Inicialización, respaldo y restauración de SQL Server
tests/
├── ApiREST_UTA.Tests/  Pruebas unitarias de la API (xUnit)
└── e2e/                Prueba de punta a punta contra Docker Compose
docs/                   Esta documentación
.github/                CI, Dependabot y plantillas
PromocionDocente.sln    Solución con la API y sus pruebas
```

## Comandos frecuentes

| Tarea | Comando |
|-------|---------|
| Compilar la API | `dotnet build PromocionDocente.sln` |
| Pruebas de la API | `dotnet test PromocionDocente.sln` |
| Formatear la API | `dotnet format PromocionDocente.sln` |
| Verificar formato (como la CI) | `dotnet format PromocionDocente.sln --verify-no-changes` |
| Lint del frontend | `cd frontend && npm run lint` |
| Pruebas del frontend | `cd frontend && npm test` |
| Punta a punta | `docker compose down -v && docker compose up -d --build && bash tests/e2e/api-smoke.sh` |
| Reiniciar la base | `docker compose down -v && docker compose up -d` |

## Convenciones de código

- **C#**: estilo definido en `.editorconfig` y verificado con `dotnet format`. Nombres de dominio en español (`SolicitudService`, `ObtenerPorIdAsync`), como el resto de la API. Consultas SQL siempre parametrizadas.
- **TypeScript**: ESLint (`frontend/eslint.config.js`) con reglas de TypeScript y de hooks de React. Los archivos `.tsx` solo exportan componentes; las funciones compartidas van en módulos `.ts`.
- **Textos de la interfaz**: en español, en frases normales (sin mayúsculas sostenidas), con botones que dicen exactamente lo que hacen ("Aprobar promoción a Agregado 2").
- **Errores de negocio en la API**: lanzar `ReglaNegocioException` con el código HTTP adecuado y un mensaje para el usuario final.

## Flujo de trabajo Git

Resumen; el detalle está en [CONTRIBUTING](../CONTRIBUTING.md).

- `main`: versiones estables. `develop`: integración. Trabajo en `feature/*`, `fix/*`, `refactor/*`, `chore/*`, `docs/*`.
- Commits pequeños con [Conventional Commits](https://www.conventionalcommits.org/es/v1.0.0/) (`feat:`, `fix:`, `test:`, `docs:`, `chore:`…).
- Merge a `develop` con `--no-ff` cuando la rama pasa lint, pruebas y build.
