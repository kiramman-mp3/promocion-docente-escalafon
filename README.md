# Promoción docente – Escalafón UTA

Sistema para revisar si un docente de la Universidad Técnica de Ambato cumple los requisitos para subir al siguiente rol del escalafón. Reúne la información de cinco direcciones (cada una es una base de datos):

| Base  | Contenido                                                          |
|-------|--------------------------------------------------------------------|
| DITIC | Usuarios del sistema (login)                                       |
| RRHH  | Tiempo en el rol actual y catálogo de roles del escalafón          |
| DIDE  | Obras publicadas, investigaciones y participación en proyectos     |
| DEaDV | Cursos de capacitación, evaluaciones docentes, capacitación impartida |
| DAC   | Tesis doctorales dirigidas                                         |

## Estructura

```
ApiREST_UTA/   API REST .NET 8 (JWT, Swagger, CORS)
frontend/      React + Vite + TypeScript
database/      Scripts SQL: schema/, security/ (login de solo lectura), seed/ (datos de prueba)
docker/        Script de inicialización de SQL Server
docker-compose.yml
```

## Puesta en marcha con Docker (recomendado)

Requisitos: Docker Desktop.

```bash
cp .env.example .env      # y cambia las contraseñas y JWT_KEY
docker compose up -d --build
```

| Servicio   | URL                                   |
|------------|---------------------------------------|
| Frontend   | http://localhost:8080                 |
| API        | http://localhost:5270                 |
| Swagger    | http://localhost:5270/swagger         |
| SQL Server | `localhost,1433` (usuario `sa`)       |

El contenedor `db-init` crea las 5 bases, el login de solo lectura de la API y carga los datos de prueba. Los scripts son idempotentes: `docker compose run --rm db-init` los vuelve a aplicar sin duplicar datos. Para empezar de cero: `docker compose down -v`.

### Usuarios de prueba

Todos usan la contraseña `Uta2026*`.

| Correo                     | Rol           | Cédula     | Situación de prueba                                   |
|----------------------------|---------------|------------|-------------------------------------------------------|
| admin@uta.edu.ec           | Administrador | –          |                                                       |
| rrhh@uta.edu.ec            | RRHH          | –          |                                                       |
| ana.perez@uta.edu.ec       | Docente       | 1801234567 | Agregado 1, cumple todo para Agregado 2; tiene una solicitud pendiente |
| carlos.gomez@uta.edu.ec    | Docente       | 1809876543 | Auxiliar 2, le faltan 4 requisitos para Agregado 1    |
| lucia.mora@uta.edu.ec      | Docente       | 1712345678 | Principal 1, le falta tesis doctoral para Principal 2 |

## Roles y flujo de aprobación

Todos los usuarios entran por el mismo login; lo que ven depende de su rol.

| Rol           | Puede                                                                                       |
|---------------|---------------------------------------------------------------------------------------------|
| Docente       | Ver **solo su propio expediente** (la API rechaza otras cédulas) y solicitar su promoción. Ver el estado de sus solicitudes y el motivo de cada documento rechazado. |
| RRHH          | Ver la bandeja de solicitudes, consultar cualquier docente, aprobar o rechazar cada documento del expediente y aprobar o rechazar la solicitud. |
| Administrador | Todo lo de RRHH, más crear usuarios, cambiar su rol o cédula, activarlos o desactivarlos y restablecer contraseñas. |

Reglas del flujo:

1. Una cédula solo puede tener una solicitud **pendiente** a la vez, y no se puede volver a pedir una promoción ya aprobada.
2. Rechazar un documento o una solicitud exige escribir el motivo, que el docente ve.
3. Una solicitud no se puede aprobar mientras tenga documentos rechazados.
4. Las solicitudes y revisiones se guardan en DITIC (`SolicitudesPromocion`, `RevisionesDocumento`). El login de la API solo puede escribir en esas tablas y en `Usuarios`; las bases de las demás direcciones siguen siendo de solo lectura. Por eso, al aprobar una promoción, **RRHH debe registrar el nuevo rol en su propio sistema**.

Endpoints nuevos: `GET /api/roles`, `GET|POST /api/solicitudes`, `GET /api/solicitudes/{id}`, `PUT /api/solicitudes/{id}/documentos`, `PUT /api/solicitudes/{id}/resolucion`, `GET|POST /api/usuarios`, `PUT /api/usuarios/{id}`, `PUT /api/usuarios/{id}/contrasena`. Todos aparecen en Swagger.

## Desarrollo local (sin contenedores para API y frontend)

1. Levanta solo la base de datos: `docker compose up -d sqlserver db-init`
2. API (requiere .NET 8 SDK). Lee el `.env` de la raíz automáticamente:
   ```bash
   cd ApiREST_UTA
   dotnet run --launch-profile http     # http://localhost:5270/swagger
   ```
3. Frontend (requiere Node 20+):
   ```bash
   cd frontend
   cp .env.example .env
   npm install
   npm run dev                         # http://localhost:5173
   ```

## Variables de entorno

Todas viven en `.env` (ignorado por git). Ver `.env.example`.

| Variable            | Uso                                                              |
|---------------------|------------------------------------------------------------------|
| `MSSQL_SA_PASSWORD` | Contraseña del administrador de SQL Server                       |
| `APP_DB_USER` / `APP_DB_PASSWORD` | Login de solo lectura que usa la API              |
| `DB_HOST` / `SQL_PORT` | Dónde encuentra la API a SQL Server al correr con `dotnet run` |
| `JWT_KEY`           | Clave para firmar los tokens (mínimo 32 caracteres)              |
| `CORS_ORIGINS`      | Orígenes del frontend permitidos, separados por coma             |
| `SEED_DATA`         | `true` para cargar los datos de prueba                           |
| `API_PORT` / `FRONTEND_PORT` | Puertos publicados por docker-compose                   |

## Flujo de trabajo Git

`main` (estable) ← `develop` ← `feature/*`, `fix/*`, `refactor/*`, `chore/*`, con Conventional Commits.
