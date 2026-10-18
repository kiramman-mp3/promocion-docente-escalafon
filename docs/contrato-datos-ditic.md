# Contrato de datos con las direcciones de origen

El sistema solo escribe en DITIC. El resto del expediente lo **lee** de las bases de Talento Humano (RRHH), Investigación (DIDE), Evaluación y Desarrollo Docente (DEaDV) y Académica (DAC). Este documento dice exactamente qué columnas necesita, de qué tipo, con qué valores y para qué regla, para acordarlo con cada dirección.

En desarrollo y en las pruebas, esas bases se crean con `database/schema/02-05*.sql`. En producción, lo recomendable es que cada dirección publique **vistas de solo lectura** con estos nombres y columnas, y que la DITIC conceda `SELECT` sobre ellas al login de la API.

## Reglas generales

- La clave entre bases es la **cédula**: `CHAR(10)`, solo dígitos, sin guiones.
- Las fechas son `DATE` o `DATETIME` sin zona horaria, en hora de Ecuador.
- Los PDF se entregan como `VARBINARY(MAX)` con el documento completo; si no hay documento, `NULL`.
- Los catálogos (tipos, roles, niveles) usan exactamente los valores de este documento. Un valor distinto se trata como desconocido y la regla no lo cuenta.
- Los `Id` deben ser estables: la revisión documental guarda el `Id` del documento revisado.

## RRHH · Dirección de Talento Humano

| Tabla o vista | Columna | Tipo | Uso |
|---------------|---------|------|-----|
| `Roles` | `Id`, `Nombre` | `INT`, `NVARCHAR(50)` | Escalafón del CES art. 63. `Id` 1 a 8 de Auxiliar 1 a Principal 3 |
| | `Categoria`, `Nivel`, `Grado` | `NVARCHAR(20)`, `INT`, `INT` | Categoría y nivel de cada grado |
| | `PermanenciaAnios` | `INT NULL` | Permanencia mínima (R2): 4, o 3 en Principal 1 y 2 |
| | `Promocionable` | `BIT` | Falso en Agregado 3 y Principal 3 (R1) |
| `TiempoRol` | `Id`, `Cedula`, `RolActual`, `FechaInicio`, `Pdf` | | Grado actual y desde cuándo (R2). `RolActual` es el `Nombre` del rol. `Pdf`: acción de personal |
| `TrayectoriaDocente` | `Cedula`, `FechaIngresoInstitucion`, `FechaIngresoTitular`, `MesesNoTitular`, `MesesTecnicoDocente`, `IngresoConPhD` | | Transitoria quinta: experiencia acumulada antes del 07-11-2012 (R2) y equivalencia de tesis con PhD (R8) |
| `CargosAutoridad` | `Cedula`, `Cargo`, `TipoCargo`, `FechaInicio`, `FechaFin`, `Pdf` | `TipoCargo`: `Rector`, `Vicerrector`, `AutoridadOrganismoSES`, `AutoridadAcademica` | Excepciones del art. 77 f (R3, R4, R5) |
| `Sabaticos` | `Cedula`, `FechaInicio`, `FechaFin`, `InformeAprobado` | | Reducción del art. 77 e (R5). Solo cuenta con el informe aprobado |

## DIDE · Dirección de Investigación y Desarrollo

| Tabla o vista | Columnas | Valores | Uso |
|---------------|----------|---------|-----|
| `Obras` | `Id`, `Cedula`, `Titulo`, `FechaPublicacion`, `NombreRevista`, `Idioma`, `FiliacionUTA`, `Pdf` | | Obras de relevancia (R3) |
| | `TipoObra` | `ArticuloArbitrado`, `Libro`, `CapituloLibro`, `Ponencia`, `PropiedadIndustrial`, `ProduccionArtistica`, `DisenoSoftwarePrototipo`, `Otra` | Criterio aplicable de los arts. 78-87 |
| | `ISSN`, `ISBN`, `BaseIndexacion`, `RevisionPares`, `RegistroSENADI`, `ValoracionExterna` | | Requisitos de cada tipo de obra |
| `Investigaciones` | `Id`, `Cedula`, `Nombre`, `Encargado`, `Horas`, `Pdf` | | Evidencia informativa del expediente |
| `ParticipacionProyecto` | `Id`, `Cedula`, `NombreProyecto`, `Rol`, `TipoProyecto`, `MesesParticipacion`, `FechaInicio`, `FechaFin` | `Rol`: `Director`, `Codirector`, `Participante`. `TipoProyecto`: `Investigacion`, `Vinculacion` | Meses en proyectos (R6) con ponderación del art. 36 c.2 y dirección (R7) |

## DEaDV · Dirección de Evaluación y Desarrollo Docente

| Tabla o vista | Columnas | Valores | Uso |
|---------------|----------|---------|-----|
| `EvaluacionesDocentes` | `Id`, `Cedula`, `Periodo`, `PeriodoAcademico`, `Universidad`, `Carrera`, `Puntaje`, `EjercioDocencia`, `Pdf` | `Puntaje` 0-100. `PeriodoAcademico` como `PAO 2024-1` | Las 4 últimas evaluaciones con docencia (R4) y alerta de UTA-EV art. 15 |
| `CursosCapacitacion` | `Id`, `Cedula`, `NombreCurso`, `Universidad`, `Facultad`, `FechaEvaluacion`, `Puntaje`, `Horas`, `TipoCapacitacion`, `FechaInicio`, `FechaFin` | `TipoCapacitacion`: `Pedagogica`, `Cientifica`, `CongresoInternacional` | 96 horas en 3 años con 25 % pedagógico (R5). Se toma `FechaFin` (o `FechaEvaluacion`) como fecha del curso |
| `Equivalencias` | `Id`, `Cedula`, `Tipo`, `Fecha`, `Meses`, `Descripcion`, `Pdf` | `Tipo`: `EvaluadorExternoConcurso`, `ParEvaluadorProyectos`, `FacilitadorCESGradoMaestria`, `FacilitadorCESDoctorado`, `FacilitadorCACES`, `EvaluacionAcademicaCES` | Horas equivalentes del art. 77 g-i (R5) y evaluaciones del CES del art. 77 j (R8) |
| `CapacitacionImpartida` | `Id`, `Cedula`, `NombreCurso`, `Horas` | | Informativa |

## DAC · Dirección Académica

| Tabla o vista | Columnas | Valores | Uso |
|---------------|----------|---------|-----|
| `TrabajosTitulacionDirigidos` | `Id`, `Cedula`, `Titulo`, `Nivel`, `Rol`, `FechaAprobacion`, `Universidad`, `Pdf` | `Nivel`: `Grado`, `Maestria`, `Doctorado`. `Rol`: `Director`, `Codirector` | Dirección de trabajos de titulación (R8) |

## Qué pasa si falta un dato

| Situación | Efecto |
|-----------|--------|
| La cédula no está en `TiempoRol` | El expediente responde 404 y no se puede solicitar |
| El `RolActual` no coincide con un `Nombre` de `Roles` | El dictamen no se puede calcular (error de datos) |
| Falta `Horas` o `TipoCapacitacion` de un curso | El curso no suma horas ni porcentaje pedagógico |
| Falta `EjercioDocencia` | Se asume que sí dio clases |
| Falta un criterio de la obra (por ejemplo `ISSN` en un artículo) | La obra se muestra como no relevante con el motivo |
| No hay `TrayectoriaDocente` | No se aplica la transitoria quinta |

Cuando Talento Humano detecta un dato equivocado durante la revisión documental, rechaza el documento con el motivo; la corrección se hace en el sistema de la dirección que lo administra.
