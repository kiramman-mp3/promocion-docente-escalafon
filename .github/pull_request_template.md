## Qué cambia

<!-- Describe el cambio en una o dos frases. -->

## Por qué

<!-- Problema que resuelve o issue relacionado (Closes #...). -->

## Cómo se probó

- [ ] `dotnet format PromocionDocente.sln --verify-no-changes`
- [ ] `dotnet test PromocionDocente.sln`
- [ ] `cd frontend && npm run lint && npm run typecheck && npm test && npm run build`
- [ ] `bash tests/e2e/api-smoke.sh` sobre una base recién creada (si cambia la API, la base o el flujo)
- [ ] Probado en el navegador con los roles afectados

## Checklist

- [ ] Commits con Conventional Commits y sin mezclar cambios no relacionados
- [ ] Pruebas agregadas o actualizadas
- [ ] Documentación actualizada (`docs/`, `README.md`) si cambia el comportamiento
- [ ] Entrada en `CHANGELOG.md` (*Sin publicar*)
- [ ] Sin secretos ni datos reales en el código
