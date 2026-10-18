# Problemas conocidos

Pendientes identificados que todavía no se corrigen. Las pruebas de `PromocionServiceTests` documentan el comportamiento **actual**; al corregir una regla hay que actualizar su prueba.

## Reglas de cálculo de la promoción (`PromocionService`)

| # | Problema | Efecto | Corrección sugerida |
|---|----------|--------|---------------------|
| 1 | `ValidarTesisDoctorado` devuelve `false` siempre que el rol objetivo tiene `Id` ≥ 6, sin consultar DAC. | Desde Principal 1 nadie cumple el requisito de tesis, aunque DAC registre tesis (caso de Lucía en los datos de prueba). | Usar `ITesisRepository` y un mínimo configurable por rol. |
| 2 | La regla de tesis está escrita como `rolDestinoId < 6`, pero el comentario dice que aplica a Principal 2 y 3 (Id 7 y 8). | Se exige también para Principal 1. | Agregar `RequiereTesisDoctorales` a `RRHH.dbo.Roles`. |
| 3 | La capacitación suma el **puntaje** de los cursos como si fueran horas. | Un curso con puntaje 40 cuenta como 40 horas. | Agregar la columna `Horas` a `CursosCapacitacion` y sumarla. |
| 4 | La investigación usa "20 horas = 1 mes" en lugar de los meses ponderados que ya calcula `ProyectoPonderadoRepository`. | Ignora el rol en el proyecto (coordinador ×2, subrogante ×1,5). | Usar `ObtenerMesesPonderadosAsync`. |
| 5 | `AniosEnRol` resta solo los años (`2026 - 2023 = 3`). | Quien empezó en diciembre de 2023 ya cuenta 3 años en enero de 2026. | Calcular con la fecha completa. |
| 6 | La evaluación promedia **todas** las evaluaciones y exige "al menos 4" escrito en el código. | No refleja "los últimos N periodos" del reglamento. | Tomar los últimos N periodos, con N configurable. |
| 7 | No se validan la obra en idioma extranjero, la capacitación impartida ni la filiación UTA, aunque los datos existen. | Requisitos del reglamento sin verificar. | Agregarlos a `RequisitoResponse` y a la tabla `Roles`. |
| 8 | `POST /api/promocion/validar` responde **500** si la cédula no está en RRHH o si ya está en el último rol. | Error genérico para un caso esperado. | Lanzar `ReglaNegocioException` (404 / 400). |
| 9 | Los requisitos de `seed/02-rrhh.sql` son ilustrativos. | Los dictámenes no son oficiales. | Cargar los valores del reglamento de la UTA. |

## Arquitectura

- Algunas interfaces del dominio dependen de DTOs de la capa de aplicación (`IObraRepository` devuelve `ObraResponse`, `IRRHHRepository` devuelve `TiempoRolResponse`).
- `IRRHHRepository` tiene el namespace duplicado `ApiREST_UTA.Domain.Interfaces.ApiREST_UTA.Domain.Interfaces`.
- `PromocionService` y `PromocionController` no tienen namespace.
- Hay código sin uso: `AppDbContext` vacío (EF Core referenciado pero no usado), `Application/Services/RequisitoRepository.cs`, `IRequisitoRepository`, `RequisitoPromocion`, `Obra` y `EvaluacionDocenteService`.
- Los endpoints de listas de importación responden 404 cuando no hay registros en vez de una lista vacía.
- Los PDF viajan en base64 dentro de las listas; con muchos documentos conviene un endpoint de descarga aparte.

## Flujo de aprobación

- Aprobar una promoción no actualiza RRHH (es una base externa de solo lectura). RRHH debe registrar el nuevo rol en su sistema.
- No hay notificaciones por correo al docente cuando su solicitud cambia de estado.
- Ver también las limitaciones de [seguridad](seguridad.md#limitaciones-conocidas).
