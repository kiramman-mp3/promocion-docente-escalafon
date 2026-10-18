# Seguridad

## Autenticación

- Contraseñas guardadas con **BCrypt** (coste 11). La API nunca devuelve el hash.
- Token **JWT HS256** firmado con `JWT_KEY` (32 caracteres o más; sin ella la API no arranca). Emisor y audiencia: `ApiREST_UTA`. Vigencia: 8 horas.
- Claims: identificador del usuario, correo, rol y, para docentes, la cédula.
- Un usuario desactivado no puede iniciar sesión.

## Autorización

| Mecanismo | Dónde | Qué protege |
|-----------|-------|-------------|
| `[Authorize]` | Todos los controladores salvo login y catálogo de roles | Exige un token válido |
| `[Authorize(Roles = "Administrador,RRHH")]` | Revisión y resolución de solicitudes | Solo revisores |
| `[Authorize(Roles = "Administrador")]` | `UsuariosController` | Solo administradores |
| `[SoloCedulaPropia]` | Importación, validación y creación de solicitudes | Un docente solo puede usar su propia cédula |
| `SolicitudService.ObtenerAsync` y `ListarAsync` | Lectura de solicitudes | Un docente solo ve las suyas |

El frontend oculta las secciones que el rol no puede usar, pero **la API vuelve a validar cada permiso**.

## Datos y secretos

- Los secretos están en `.env` (ignorado por git). `appsettings.json` no tiene cadenas de conexión ni claves.
- La API se conecta con un login de **permisos mínimos**: lectura en las 5 bases y escritura solo en tres tablas de DITIC, sin `DELETE`.
- Todas las consultas SQL usan parámetros.
- CORS solo permite los orígenes de `CORS_ORIGINS`.
- La API no escribe secretos ni cadenas de conexión en los logs.

## Limitaciones conocidas

| Limitación | Impacto | Mitigación posible |
|-----------|---------|--------------------|
| Los JWT no se pueden revocar | Un usuario desactivado o con rol cambiado conserva su token hasta 8 horas | Tokens más cortos con refresh token, o comprobar `Activo` en cada petición |
| El token vive en `sessionStorage` | Accesible por JavaScript si hubiera un XSS | Cookie `HttpOnly` con protección CSRF |
| Sin límite de intentos de login | Posibles ataques de fuerza bruta | Rate limiting de ASP.NET Core en `/api/Auth/login` |
| Sin redirección a HTTPS en la API | Depende del proxy | TLS en el proxy inverso |
| No se valida que `documentoId` pertenezca a la cédula de la solicitud | Un revisor podría registrar una revisión sobre un id ajeno | Validar contra la base de origen antes de guardar |
| No hay registro de auditoría aparte | Solo quedan quién y cuándo revisó o resolvió | Tabla de auditoría con cada cambio |
| La clave JWT de los primeros sprints quedó en el historial de git | Ya no se usa | No reutilizarla nunca |

Para reportar una vulnerabilidad, sigue [SECURITY.md](../SECURITY.md).
