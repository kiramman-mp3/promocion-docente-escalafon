# Cómo contribuir

Gracias por ayudar a mejorar el sistema de promoción docente. Esta guía resume cómo trabajamos.

## Antes de empezar

1. Lee la [guía de desarrollo](docs/desarrollo.md) y levanta el proyecto con `docker compose up -d --build`.
2. Revisa los [problemas conocidos](docs/problemas-conocidos.md): puede que lo que quieres arreglar ya esté descrito.
3. Si el cambio es grande, abre primero un issue para acordar el enfoque.

## Ramas

```
main        versiones estables (solo merges desde develop)
└── develop rama de integración
    ├── feature/*    nuevas funcionalidades
    ├── fix/*        correcciones
    ├── refactor/*   cambios internos sin cambiar comportamiento
    ├── chore/*      configuración, dependencias, CI
    └── docs/*       documentación
```

- Nunca trabajes directamente en `main` ni hagas `push --force` sobre `main` o `develop`.
- Crea la rama desde `develop` actualizado, con un nombre descriptivo en kebab-case: `feature/notificaciones-por-correo`, `fix/calculo-anios-en-rol`.

```bash
git checkout develop
git pull
git checkout -b feature/nombre-de-la-funcionalidad
```

## Commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/es/v1.0.0/): `tipo: descripción`.

| Tipo | Para |
|------|------|
| `feat` | Una funcionalidad nueva |
| `fix` | Una corrección |
| `refactor` | Reorganizar código sin cambiar el comportamiento |
| `test` | Pruebas |
| `docs` | Documentación |
| `style` | Formato o estilos visuales |
| `perf` | Rendimiento |
| `build` | Dependencias o sistema de build |
| `ci` | Integración continua |
| `chore` | Mantenimiento |

Commits pequeños y coherentes: cada uno debe poder revertirse sin arrastrar otros cambios. No mezcles una funcionalidad, una corrección y un cambio de formato en el mismo commit.

```
feat: add document review screen
fix: block a second pending request for the same cedula
test: cover request resolution rules
```

## Antes de abrir un pull request

Todo esto lo vuelve a comprobar la CI:

```bash
# API
dotnet format PromocionDocente.sln --verify-no-changes
dotnet test PromocionDocente.sln

# Frontend
cd frontend
npm run lint
npm run typecheck
npm test
npm run build

# Punta a punta (si tocaste la API, la base o el flujo)
docker compose down -v && docker compose up -d --build && bash tests/e2e/api-smoke.sh
```

## Pull requests

- Contra `develop`. Completa la plantilla: qué cambia, por qué y cómo se probó.
- Agrega o actualiza pruebas cuando cambies reglas de negocio o permisos.
- Si cambias la API, actualiza [docs/api.md](docs/api.md). Si cambias tablas, actualiza los scripts de `database/` y [docs/base-de-datos.md](docs/base-de-datos.md).
- Anota el cambio en la sección *Sin publicar* de [CHANGELOG.md](CHANGELOG.md).
- La rama se integra con `git merge --no-ff` cuando la CI está en verde.

## Publicar una versión

1. En `develop`, mueve lo de *Sin publicar* del CHANGELOG a una versión nueva con fecha.
2. `git checkout main && git merge --no-ff develop`
3. `git tag vX.Y.Z && git push origin main --tags`

## Estilo

- C#: `.editorconfig` + `dotnet format`. Nombres de dominio en español, como el resto de la API. SQL siempre parametrizado.
- TypeScript: ESLint. Los archivos `.tsx` solo exportan componentes.
- Textos de la interfaz en español y en frases normales; los botones dicen lo que hacen.
