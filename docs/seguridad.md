# Seguridad

## Autenticación

- Contraseñas guardadas con **BCrypt** (coste 11). La API nunca devuelve el hash.
- Token **JWT HS256** firmado con `JWT_KEY` (32 caracteres o más; sin ella la API no arranca). Emisor y audiencia: `ApiREST_UTA`. Vigencia: 8 horas.
- Claims: identificador del usuario, correo, rol, la cédula (docentes) y `comision=true` para los miembros vigentes de la Comisión Académica.
- **Revocación inmediata**: en cada petición la API comprueba que el usuario siga activo. Desactivar una cuenta corta su acceso en la siguiente petición, aunque su token no haya vencido.
- **Límite de intentos de login**: ventana fija de un minuto por IP (`LOGIN_RATE_LIMIT`, 10 por defecto). Al superarlo responde **429**.

## Autorización

| Mecanismo | Dónde | Qué protege |
|-----------|-------|-------------|
| `[Authorize]` | Todos los controladores salvo login, roles, requisitos (lectura) y `/health` | Exige un token válido de un usuario activo |
| `[Authorize(Roles = "Administrador,RRHH")]` | Revisión documental, envío a la Comisión, acción de personal | Talento Humano |
| `[Authorize(Roles = "Administrador")]` | Usuarios, requisitos (escritura), integrantes de la Comisión, apelación ante el HCU, auditoría y reportes | DITIC |
| `[SoloCedulaPropia]` | Importación, dictamen y presentación de solicitudes | Un docente solo usa su propia cédula (la Comisión puede consultar cualquiera) |
| `ClaimsUsuario.PuedeVerSolicitud` | Detalle, evidencias y acto de una solicitud | El docente ve las suyas; la Comisión, las que ya le enviaron; Talento Humano y DITIC, todas |
| `SolicitudService.VotarAsync` | Votación | Solo miembros vigentes; nadie vota su propia solicitud (conflicto de interés) |
| `SolicitudService.CerrarVotacionAsync` | Cierre | Solo quien preside |

El frontend oculta las secciones que el usuario no puede usar, pero **la API vuelve a validar cada permiso**.

## Integridad de los documentos

- Las evidencias se aceptan solo si el contenido empieza con `%PDF-` (no basta la extensión) y pesan hasta 10 MB. Se guarda su hash SHA-256.
- El acto administrativo se genera en el servidor y se guarda con su hash SHA-256, que se muestra junto al botón de descarga para verificar que el archivo no fue alterado.
- El dictamen con el que decidió la Comisión queda congelado en `DictamenJson`.
- Evidencias y auditoría solo admiten `INSERT` para el login de la API: no se pueden editar ni borrar desde la aplicación.

## Auditoría

`AuditoriaFilter` registra en `DITIC.dbo.Auditoria` toda operación que cambia datos y cada descarga de documentos: fecha, usuario, IP, `Controlador.Acción`, método, ruta, código de respuesta y argumentos. Las contraseñas y tokens se enmascaran (`***`). Si la bitácora falla, la operación continúa y el error queda en el log. El Administrador la consulta en *Operación* o en `GET /api/auditoria`.

## Datos y secretos

- Los secretos están en `.env` (ignorado por git). `appsettings.json` no tiene cadenas de conexión ni claves.
- La API se conecta con un login de **permisos mínimos**: lectura en las 5 bases y escritura solo en tablas de DITIC, sin `DELETE`.
- Todas las consultas SQL usan parámetros.
- CORS solo permite los orígenes de `CORS_ORIGINS`.
- La API no escribe secretos ni cadenas de conexión en los logs.

## Pruebas de seguridad

El workflow de CI ejecuta **OWASP ZAP** (escaneo base del frontend y escaneo de la API con su OpenAPI) y deja los reportes como artefactos. Ver [pruebas](pruebas.md).

## Limitaciones conocidas

| Limitación | Impacto | Mitigación posible |
|-----------|---------|--------------------|
| El token vive en `sessionStorage` | Accesible por JavaScript si hubiera un XSS | Cookie `HttpOnly` con protección CSRF |
| Un cambio de rol se refleja en el siguiente login | El rol del token anterior sigue hasta que venza | Comparar el rol del token con el de la base en `OnTokenValidated` |
| El límite de login es por IP | Detrás de un proxy todos comparten IP | Configurar `ForwardedHeaders` con los proxies confiables |
| Sin redirección a HTTPS en la API | Depende del proxy | TLS en el proxy inverso |
| El acto no lleva firma electrónica | El PDF se verifica por su hash, no con FirmaEC | Integrar la firma electrónica de las autoridades |
| La clave JWT de los primeros sprints quedó en el historial de git | Ya no se usa | No reutilizarla nunca |

Para reportar una vulnerabilidad, sigue [SECURITY.md](../SECURITY.md).
