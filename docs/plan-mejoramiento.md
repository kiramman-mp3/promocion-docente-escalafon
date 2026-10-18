# Plan de mejoramiento

Exigido por el Reglamento de la DITIC (arts. 18 a y 27). Recoge lo que falta para que el sistema pase de la versión actual, que cumple el procedimiento con decisiones propuestas, a un servicio institucional en producción. Se revisa al cerrar cada sprint.

## Decisiones que debe tomar la universidad

| # | Pendiente | Responsable | Mientras tanto |
|---|-----------|-------------|----------------|
| M1 | Expedir la normativa interna de promoción con los valores de cada grado (UTA, transitoria cuarta) | Consejo Universitario | Requisitos propuestos marcados como tales; el Administrador carga la versión oficial en *Requisitos* sin tocar el código |
| M2 | Confirmar quórum, mayoría y voto dirimente de la Comisión Académica | Consejo Universitario | Quórum 4, mayoría simple, dirimente de la presidencia (D3) |
| M3 | Confirmar órgano y término de la apelación | Procuraduría | Consejo Universitario, 10 días hábiles (D4) |
| M4 | Designar a los integrantes reales de la Comisión | Rectorado / HCU | Integrantes de prueba |
| M5 | Acordar el [contrato de datos](contrato-datos-ditic.md) con RRHH, DIDE, DEaDV y DAC | DITIC con cada dirección | Bases de prueba con las columnas del contrato |

## Mejoras técnicas

| # | Mejora | Por qué | Prioridad |
|---|--------|---------|-----------|
| T1 | Firma electrónica del acto con FirmaEC | El PDF hoy se verifica por su huella SHA-256, no por firma (CES art. 77 l) | Alta |
| T2 | Notificar por correo cada cambio de estado y el inicio del término de apelación | El docente hoy tiene que entrar al sistema para enterarse; la notificación inicia el plazo del COA | Alta |
| T3 | Registrar el nuevo grado en RRHH al ejecutar la promoción (servicio o vista de escritura acordada) | Hoy Talento Humano lo registra dos veces | Media |
| T4 | Guardar en la auditoría los valores antes y después de cada cambio | Hoy se guardan la acción y sus argumentos | Media |
| T5 | Tomar la IP real detrás del proxy (`ForwardedHeaders`) | El límite de login y la auditoría usan la IP del proxy | Media, al desplegar |
| T6 | Invalidar el token cuando cambia el rol, no solo al desactivar | El rol del token anterior dura hasta que venza | Media |
| T7 | Token en cookie `HttpOnly` con protección CSRF | Reduce el impacto de un XSS | Media |
| T8 | Mover quórum, mayoría y plazo de apelación a configuración | Hoy cambiarlos requiere un despliegue | Baja, tras M2 y M3 |
| T9 | Copiar los respaldos fuera del servidor y probar la restauración cada mes | El volumen de respaldos está en el mismo disco | Alta, al desplegar |
| T10 | Pruebas de interfaz automatizadas (Playwright) del trámite completo | Hoy el frontend tiene pruebas de componentes y una revisión visual manual | Baja |

## Seguimiento

| Indicador | Fuente | Meta |
|-----------|--------|------|
| Disponibilidad | `GET /health` monitoreado | 99 % en horario laboral |
| Días promedio desde la solicitud hasta la resolución | *Operación* → reporte de uso | Menos de 30 días |
| Errores del servidor (5xx) en 30 días | Reporte de uso | 0 sin investigar |
| Respaldo verificado | Log de `db-backup` | Diario |
| CI en verde en `develop` | GitHub Actions | Siempre |
| Umbrales de k6 (p95 < 800 ms, errores < 1 %) | Workflow *Rendimiento* | Cada semana |
