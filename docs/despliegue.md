# Despliegue

## Servicios de Docker Compose

| Servicio | Imagen | Puerto | Función |
|----------|--------|--------|---------|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | `SQL_PORT` (1433) | Base de datos, con volumen `sqlserver-data` y healthcheck |
| `db-init` | la misma imagen | — | Contenedor de un solo uso: ejecuta `docker/init-db.sh` y termina |
| `api` | `ApiREST_UTA/Dockerfile` (SDK 8 → ASP.NET 8, usuario sin privilegios) | `API_PORT` (5270) → 8080 | API; arranca cuando `db-init` terminó bien |
| `frontend` | `frontend/Dockerfile` (Node 20 → nginx 1.27) | `FRONTEND_PORT` (8080) | Archivos estáticos del frontend |

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
| Requisitos del escalafón | Cargar en `RRHH.dbo.Roles` los valores oficiales del reglamento. |
| Copias de seguridad | Programar respaldos de DITIC, que es la única base donde escribe el sistema. |

### Crear el primer administrador sin datos de prueba

Genera un hash BCrypt de la contraseña (por ejemplo con `node -e "console.log(require('bcryptjs').hashSync('TuClave', 11))"`) e insértalo con un usuario con permisos de escritura:

```sql
INSERT INTO DITIC.dbo.Usuarios (Correo, Contrasena, Nombre, Apellido, Rol)
VALUES (N'admin@uta.edu.ec', N'<hash BCrypt>', N'Nombre', N'Apellido', N'Administrador');
```

Desde ahí, el resto de cuentas se crean en la sección *Usuarios*.
