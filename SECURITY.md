# Política de seguridad

## Versiones con soporte

Solo la rama `main` (la última versión publicada) recibe correcciones de seguridad.

## Reportar una vulnerabilidad

**No abras un issue público** para reportar una vulnerabilidad.

Usa el reporte privado de GitHub: pestaña **Security** del repositorio → **Report a vulnerability**. Incluye:

- qué componente se ve afectado (API, frontend, scripts de base de datos, CI);
- cómo reproducirlo, paso a paso;
- el impacto que esperas (por ejemplo: un docente puede ver el expediente de otro).

Respondemos en un plazo de 7 días hábiles y avisamos cuando la corrección esté publicada.

## Buenas prácticas para quien despliega

- No uses los usuarios ni las contraseñas de prueba fuera de desarrollo (`SEED_DATA=false`).
- Genera secretos nuevos para `MSSQL_SA_PASSWORD`, `APP_DB_PASSWORD` y `JWT_KEY`, y no los guardes en el repositorio.
- Publica la API detrás de HTTPS, con `CORS_ORIGINS` limitado al dominio del frontend y Swagger desactivado.

Los detalles de autenticación, permisos y limitaciones conocidas están en [docs/seguridad.md](docs/seguridad.md).
