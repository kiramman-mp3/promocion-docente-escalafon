# Promoción docente – Escalafón UTA

[![CI](https://github.com/kiramman-mp3/promocion-docente-escalafon/actions/workflows/ci.yml/badge.svg?branch=develop)](https://github.com/kiramman-mp3/promocion-docente-escalafon/actions/workflows/ci.yml)
[![Licencia: MIT](https://img.shields.io/badge/licencia-MIT-blue.svg)](LICENSE)

Sistema para el trámite de promoción del personal académico titular de la Universidad Técnica de Ambato, conforme al Reglamento de Carrera y Escalafón del CES (RPC-SE-19-No.055-2021) y a la normativa de la UTA (0153-HCU-2026).

Reúne en un solo expediente lo que cada dirección guarda en su propia base de datos (RRHH, DIDE, DEaDV y DAC), calcula un dictamen que cita el artículo de cada requisito y lleva la solicitud desde la presentación hasta la acción de personal, pasando por la revisión de Talento Humano, la votación de la Comisión Académica y, si hace falta, la apelación ante el Consejo Universitario.

## Funcionalidades

- **Dictamen normativo**: permanencia, obras de relevancia, evaluación, capacitación, proyectos, dirección de proyectos y trabajos de titulación, con lo obtenido frente a lo exigido, el artículo y las excepciones del art. 77 y de la transitoria quinta.
- **Requisitos versionados**: el Administrador carga cada versión con su resolución; cada solicitud se evalúa con la que regía al presentarla.
- **Trámite completo**:
  - el docente presenta la solicitud y anexa evidencias en PDF;
  - Talento Humano revisa cada documento y envía el expediente a la Comisión;
  - la Comisión vota con quórum, motivación, certificación presupuestaria y voto dirimente;
  - el sistema emite la resolución en PDF con su huella SHA-256;
  - Talento Humano registra la acción de personal;
  - el docente puede apelar en 10 días hábiles y el Administrador registra lo que resuelve el HCU.
- **Un login para todos**: Docente, Talento Humano, Autoridad y Administrador; la pertenencia a la Comisión Académica se suma a cualquiera de ellos.
- **Operación**: bitácora de auditoría, reporte de utilización, `/health`, respaldos verificados, límite de intentos de login y revocación inmediata de accesos.

La correspondencia artículo por artículo está en la [matriz de cumplimiento normativo](docs/cumplimiento-normativo.md).

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
| Estado | http://localhost:5270/health |
| SQL Server | `localhost,1433` |

### Usuarios de prueba

Contraseña de todos: `Uta2026*`.

| Correo | Rol | Situación |
|--------|-----|-----------|
| `admin@uta.edu.ec` | Administrador | Usuarios, requisitos, Comisión, operación y apelaciones |
| `rrhh@uta.edu.ec` | RRHH | Tiene en revisión documental la solicitud de Ana; integra la Comisión |
| `vicerrectorado@uta.edu.ec` | Autoridad | Preside la Comisión Académica |
| `financiero@uta.edu.ec` | Autoridad | Director Financiero: certifica el presupuesto |
| `ana.perez@uta.edu.ec` | Docente | Agregado 1, cumple todo para Agregado 2 |
| `carlos.gomez@uta.edu.ec` | Docente | Auxiliar 2, aún sin la permanencia mínima |
| `lucia.mora@uta.edu.ec` | Docente | Principal 1, cumple para Principal 2; integra la Comisión |
| `jorge.ruiz@uta.edu.ec` | Docente | Agregado 3: pasa a Principal por concurso |
| `marta.villacres@uta.edu.ec` | Docente | Agregado 2, cumple gracias a las excepciones |

Los demás usuarios y escenarios están en [base de datos](docs/base-de-datos.md#datos-de-prueba). Para desarrollar sin contenedores (API con `dotnet run`, frontend con `npm run dev`), sigue la [guía de desarrollo](docs/desarrollo.md).

## Tecnologías

| Capa | Tecnología |
|------|-----------|
| Frontend | React 18, TypeScript, Vite 5 |
| API | ASP.NET Core 8, ADO.NET, JWT, BCrypt, Swashbuckle |
| Base de datos | SQL Server 2022 (5 bases: DITIC, RRHH, DIDE, DEaDV, DAC) |
| Pruebas | xUnit, Moq, Vitest, Testing Library, prueba de punta a punta, k6, OWASP ZAP |
| Calidad | `dotnet format`, ESLint, TypeScript estricto, GitHub Actions, Dependabot |

## Estructura

```
ApiREST_UTA/          API REST .NET 8
frontend/             Aplicación React
database/             Scripts SQL: esquema, login de la API y datos de prueba
docker/               Inicialización, respaldo y restauración de SQL Server
tests/                Pruebas de la API (xUnit), de punta a punta y de carga
docs/                 Documentación
.github/              CI, prueba de rendimiento, Dependabot y plantillas
docker-compose.yml    SQL Server + API + frontend (+ respaldos con el perfil operacion)
PromocionDocente.sln  Solución de la API y sus pruebas
```

## Pruebas

```bash
dotnet test PromocionDocente.sln                     # API: 125 pruebas
cd frontend && npm test                              # frontend: 31 pruebas
docker compose down -v && LOGIN_RATE_LIMIT=100 docker compose up -d --build && bash tests/e2e/api-smoke.sh   # punta a punta: 86 comprobaciones
```

GitHub Actions ejecuta lint, pruebas, build, la prueba de punta a punta, un respaldo verificado y OWASP ZAP en cada push y pull request a `main` y `develop`; k6 corre cada semana. Detalles en [pruebas](docs/pruebas.md) y en el [plan de pruebas](docs/plan-pruebas.md).

## Documentación

| | |
|-|-|
| [Cumplimiento normativo](docs/cumplimiento-normativo.md) | [Roles y flujo de aprobación](docs/roles-y-flujo.md) |
| [Arquitectura](docs/arquitectura.md) | [Referencia de la API](docs/api.md) |
| [Base de datos](docs/base-de-datos.md) | [Contrato de datos](docs/contrato-datos-ditic.md) |
| [Frontend](docs/frontend.md) | [Guía de desarrollo](docs/desarrollo.md) |
| [Pruebas y CI](docs/pruebas.md) | [Plan de pruebas](docs/plan-pruebas.md) |
| [Despliegue](docs/despliegue.md) | [Seguridad](docs/seguridad.md) |
| [Plan de mejoramiento](docs/plan-mejoramiento.md) | [Problemas conocidos](docs/problemas-conocidos.md) |

## Contribuir

Lee [CONTRIBUTING.md](CONTRIBUTING.md): ramas `feature/*` desde `develop`, Conventional Commits y la CI en verde antes de integrar. Los cambios se registran en el [CHANGELOG](CHANGELOG.md). Para reportar vulnerabilidades, sigue [SECURITY.md](SECURITY.md).

## Licencia

[MIT](LICENSE) © 2026 kiramman-mp3
