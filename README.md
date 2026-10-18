# Promoción docente – Escalafón UTA

[![CI](https://github.com/kiramman-mp3/promocion-docente-escalafon/actions/workflows/ci.yml/badge.svg?branch=develop)](https://github.com/kiramman-mp3/promocion-docente-escalafon/actions/workflows/ci.yml)
[![Licencia: MIT](https://img.shields.io/badge/licencia-MIT-blue.svg)](LICENSE)

Sistema para revisar si un docente de la Universidad Técnica de Ambato cumple los requisitos para subir al siguiente rol del escalafón y para gestionar la aprobación de su promoción.

Reúne en un solo expediente lo que cada dirección guarda en su propia base de datos (RRHH, DIDE, DEaDV y DAC), calcula el dictamen automático y permite que RRHH apruebe o rechace cada documento antes de resolver la solicitud.

## Funcionalidades

- **Un login para tres roles**:
  - **Docente**: ve solo su expediente y solicita su promoción.
  - **RRHH**: revisa documentos y resuelve solicitudes.
  - **Administrador**: hace lo mismo que RRHH y además gestiona usuarios.
- **Expediente del docente**: tiempo en el rol, obras, evaluaciones, cursos, investigaciones, tesis y otros méritos, con visor de PDF.
- **Dictamen automático** requisito por requisito y una escalera con el rol actual y el siguiente.
- **Flujo de aprobación**:
  - cada documento se aprueba o se rechaza con un motivo;
  - una solicitud no se puede aprobar si tiene documentos rechazados.
- **API REST** documentada con Swagger, autenticación JWT y contraseñas con BCrypt.

## Puesta en marcha

Requisitos: [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
git clone https://github.com/kiramman-mp3/promocion-docente-escalafon.git
cd promocion-docente-escalafon
cp .env.example .env          # cambia las contraseñas y JWT_KEY
docker compose up -d --build
```

| Servicio | URL |
|----------|-----|
| Aplicación | http://localhost:8080 |
| API | http://localhost:5270 |
| Swagger | http://localhost:5270/swagger |
| SQL Server | `localhost,1433` |

### Usuarios de prueba

Contraseña de todos: `Uta2026*`.

| Correo | Rol | Situación |
|--------|-----|-----------|
| `admin@uta.edu.ec` | Administrador | |
| `rrhh@uta.edu.ec` | RRHH | Tiene en su bandeja la solicitud pendiente de Ana |
| `ana.perez@uta.edu.ec` | Docente | Agregado 1, cumple todo para Agregado 2 |
| `carlos.gomez@uta.edu.ec` | Docente | Auxiliar 2, le faltan 4 requisitos para Agregado 1 |
| `lucia.mora@uta.edu.ec` | Docente | Principal 1, le falta la tesis doctoral para Principal 2 |

Para desarrollar sin contenedores (API con `dotnet run`, frontend con `npm run dev`), sigue la [guía de desarrollo](docs/desarrollo.md).

## Tecnologías

| Capa | Tecnología |
|------|-----------|
| Frontend | React 18, TypeScript, Vite 5 |
| API | ASP.NET Core 8, ADO.NET, JWT, BCrypt, Swashbuckle |
| Base de datos | SQL Server 2022 (5 bases: DITIC, RRHH, DIDE, DEaDV, DAC) |
| Pruebas | xUnit, Moq, Vitest, Testing Library, prueba de punta a punta con Docker Compose |
| Calidad | `dotnet format`, ESLint, TypeScript estricto, GitHub Actions, Dependabot |

## Estructura

```
ApiREST_UTA/          API REST .NET 8
frontend/             Aplicación React
database/             Scripts SQL: esquema, login de la API y datos de prueba
docker/               Inicialización de SQL Server
tests/                Pruebas de la API (xUnit) y de punta a punta
docs/                 Documentación
.github/              CI, Dependabot y plantillas de issues y PR
docker-compose.yml    SQL Server + API + frontend
PromocionDocente.sln  Solución de la API y sus pruebas
```

## Pruebas

```bash
dotnet test PromocionDocente.sln                     # API: 46 pruebas
cd frontend && npm test                              # frontend: 32 pruebas
docker compose down -v && docker compose up -d --build && bash tests/e2e/api-smoke.sh   # punta a punta
```

GitHub Actions ejecuta lint, pruebas, build y la prueba de punta a punta en cada push y pull request a `main` y `develop`. Detalles en [docs/pruebas.md](docs/pruebas.md).

## Documentación

| | |
|-|-|
| [Arquitectura](docs/arquitectura.md) | [Roles y flujo de aprobación](docs/roles-y-flujo.md) |
| [Referencia de la API](docs/api.md) | [Base de datos](docs/base-de-datos.md) |
| [Frontend](docs/frontend.md) | [Guía de desarrollo](docs/desarrollo.md) |
| [Pruebas y CI](docs/pruebas.md) | [Despliegue](docs/despliegue.md) |
| [Seguridad](docs/seguridad.md) | [Problemas conocidos](docs/problemas-conocidos.md) |

## Contribuir

Lee [CONTRIBUTING.md](CONTRIBUTING.md): ramas `feature/*` desde `develop`, Conventional Commits y la CI en verde antes de integrar. Los cambios se registran en el [CHANGELOG](CHANGELOG.md). Para reportar vulnerabilidades, sigue [SECURITY.md](SECURITY.md).

## Licencia

[MIT](LICENSE) © 2026 kiramman-mp3
