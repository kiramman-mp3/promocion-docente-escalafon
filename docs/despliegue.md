# Despliegue

## Servicios de Docker Compose

| Servicio | Imagen | Puerto | Función |
|----------|--------|--------|---------|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | `SQL_PORT` (1433) | Base de datos, con volumen `sqlserver-data` y healthcheck |
| `db-init` | la misma imagen | — | Contenedor de un solo uso: ejecuta `docker/init-db.sh` y termina |
| `api` | `ApiREST_UTA/Dockerfile` (SDK 8 → ASP.NET 8, usuario sin privilegios) | `API_PORT` (5270) → 8080 | API; arranca cuando `db-init` terminó bien |
| `frontend` | `frontend/Dockerfile` (Node 20 → nginx 1.27) | `FRONTEND_PORT` (8080) | Archivos estáticos del frontend |
| `db-backup` | imagen de SQL Server · perfil `operacion` | — | Respaldo verificado de las 5 bases |
| `db-restore` | imagen de SQL Server · perfil `operacion` | — | Restaura una base desde un respaldo |

```bash
docker compose up -d --build     # levantar o actualizar
docker compose logs -f api       # ver logs
docker compose down              # apagar (conserva los datos)
docker compose down -v           # apagar y borrar la base
```

La URL de la API queda fija en el frontend al compilar (`VITE_API_URL`, por defecto `http://localhost:${API_PORT}`). Si la API se publica en otro dominio, recompila el frontend con ese valor.

## Antes de producción

Docker Compose está pensado para desarrollo y pruebas. Para un entorno real:

| Tema | Qué hacer |
|------|-----------|
| Secretos | Generar `MSSQL_SA_PASSWORD`, `APP_DB_PASSWORD` y `JWT_KEY` nuevos y guardarlos en un gestor de secretos, no en archivos. |
| Datos de prueba | `SEED_DATA=false` y crear el primer administrador a mano (ver abajo). |
| Entorno | `ASPNETCORE_ENVIRONMENT=Production` y `SWAGGER_ENABLED=false` (o protegerlo). |
| HTTPS | Poner la API y el frontend detrás de un proxy inverso con TLS. La API no hace redirección a HTTPS. |
| CORS | `CORS_ORIGINS` solo con el dominio real del frontend. |
| Base de datos | Usar las bases reales de cada dirección o un SQL Server administrado. La edición *Developer* no tiene licencia para producción. |
| Requisitos de promoción | Cuando la UTA apruebe su normativa interna, crear en *Requisitos* una versión por grado con el número de resolución y `EsPropuesta` desmarcado. |
| Comisión Académica | Registrar en *Comisión* a los integrantes reales (UTA art. 47) y finalizar los de prueba. |
| Límite de login | Si hay un proxy delante, configurar `ForwardedHeaders` para que el límite use la IP real del cliente. |
| Copias de seguridad | Programar `db-backup` (ver abajo) y copiar los `.bak` fuera del servidor. |
| Monitoreo | Vigilar `GET /health` (200 si las cinco bases responden, 503 si no). |

### Crear el primer administrador sin datos de prueba

Genera un hash BCrypt de la contraseña (por ejemplo con `node -e "console.log(require('bcryptjs').hashSync('TuClave', 11))"`) e insértalo con un usuario con permisos de escritura:

```sql
INSERT INTO DITIC.dbo.Usuarios (Correo, Contrasena, Nombre, Apellido, Rol)
VALUES (N'admin@uta.edu.ec', N'<hash BCrypt>', N'Nombre', N'Apellido', N'Administrador');
```

Desde ahí, el resto de cuentas se crean en la sección *Usuarios*.

## Copias de seguridad

El Reglamento de la DITIC (art. 23 e) exige respaldos verificados. `docker/backup-db.sh` hace un `BACKUP DATABASE … WITH COMPRESSION, CHECKSUM` de cada base, lo comprueba con `RESTORE VERIFYONLY` y borra los respaldos más antiguos que `RETENCION_DIAS` (30 por defecto).

```bash
docker compose --profile operacion run --rm db-backup                 # respaldar las 5 bases
docker compose --profile operacion run --rm -e BASE=DITIC db-restore  # restaurar el último respaldo de DITIC
docker compose --profile operacion run --rm -e BASE=DITIC -e ARCHIVO=DITIC_20261018_020000.bak db-restore
```

Los archivos `.bak` quedan en `/var/opt/mssql/backup`, dentro del volumen `sqlserver-data`. Ese volumen está en el mismo servidor: para que el respaldo sirva ante una falla del disco hay que copiarlo a otro lugar. Restaurar **reemplaza** la base actual y vuelve a asociar el usuario de la API.

Para programarlo, por ejemplo con cron en el servidor:

```cron
0 2 * * * cd /opt/promocion-docente && docker compose --profile operacion run --rm db-backup >> /var/log/uta-backup.log 2>&1
```

En CI se ejecuta un respaldo después de la prueba de punta a punta para comprobar que el script funciona.

## Disponibilidad y uso

- `GET /health` informa si responden las cinco bases y cuánto tardó la comprobación.
- En *Operación* el Administrador ve el estado, el reporte de utilización de los últimos 30 días y la bitácora de auditoría.
- `tests/performance/carga.js` es una prueba de carga con k6; el workflow *Rendimiento* la ejecuta cada semana y bajo demanda (ver [pruebas](pruebas.md)).
