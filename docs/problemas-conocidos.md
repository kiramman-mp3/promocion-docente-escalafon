# Problemas conocidos

Limitaciones actuales del sistema. Las mejoras previstas para cada una están en el [plan de mejoramiento](plan-mejoramiento.md).

## Normativa pendiente

- Los requisitos de promoción cargados son una **propuesta** basada en los mínimos del CES, mientras la UTA expide su normativa interna (transitoria cuarta, 0153-HCU-2026). El dictamen lo avisa.
- El quórum y la mayoría de la Comisión, y el órgano y el término de la apelación, son decisiones por defecto que debe confirmar la universidad (D3 y D4 en [cumplimiento normativo](cumplimiento-normativo.md#decisiones-tomadas-por-defecto)).

## Flujo de aprobación

- Ejecutar una promoción no actualiza RRHH (es una base externa de solo lectura). Talento Humano registra el nuevo grado en su sistema con la misma acción de personal.
- No hay notificaciones por correo: el docente debe entrar al sistema para ver los cambios y el término de apelación.
- El acto administrativo no lleva firma electrónica; se verifica por su huella SHA-256.

## Seguridad

- Ver las [limitaciones de seguridad](seguridad.md#limitaciones-conocidas).

## Arquitectura

- Algunas interfaces del dominio dependen de DTOs de la capa de aplicación (`IObraRepository` devuelve `ObraResponse`, `IRRHHRepository` devuelve `TiempoRolResponse`).
- EF Core sigue referenciado y `AppDbContext` está vacío; toda la persistencia usa ADO.NET.
- Los PDF de las bases de origen viajan en base64 dentro de las listas de importación; con muchos documentos conviene un endpoint de descarga aparte, como el de evidencias.
- La auditoría guarda los argumentos de cada operación, no los valores anteriores y posteriores.

## Corregido en esta versión

Los problemas que este documento listaba antes quedaron resueltos con el motor de reglas normativo: la tesis doctoral que nunca se cumplía, la capacitación que sumaba puntajes como horas, la investigación a "20 horas = 1 mes", los años en el rol calculados solo por año, el promedio de todas las evaluaciones, los requisitos sin validar, los errores 500 del dictamen, los requisitos ilustrativos en `RRHH.Roles`, el namespace duplicado de `IRRHHRepository`, los servicios sin namespace, el código sin uso de requisitos, las listas vacías que respondían 404, los JWT que no se podían revocar, la falta de límite de login, la revisión de documentos ajenos al expediente y la ausencia de auditoría.
