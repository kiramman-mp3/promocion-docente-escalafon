# Cumplimiento normativo

Matriz que relaciona cada disposición que aplica al sistema con la regla o pantalla que la implementa, la prueba que la verifica y su estado. Las pruebas unitarias llevan el artículo en el nombre (`Art76a_…`, `COA224_…`) para que la trazabilidad se mantenga al cambiar el código.

## Normativa considerada

| Abreviatura | Norma |
|-------------|-------|
| **CES** | Reglamento de Carrera y Escalafón del Profesor e Investigador del Sistema de Educación Superior, RPC-SE-19-No.055-2021 |
| **UTA** | Reforma al Estatuto / normativa de la Universidad Técnica de Ambato, resolución 0153-HCU-2026 |
| **UTA-EV** | Reglamento de evaluación integral del desempeño del personal académico de la UTA, 0665-CU-P-2024 |
| **DITIC** | Reglamento de la Dirección de Tecnologías de la Información y Comunicación de la UTA, 0290-HCU-2026 |
| **COA** | Código Orgánico Administrativo |

**Estados:** ✅ cumple · 🟡 cumple con una decisión pendiente de la universidad · ⏳ fuera del alcance de esta etapa (ver [plan de mejoramiento](plan-mejoramiento.md)).

Abreviaturas de las pruebas: **RP** `Promocion/ReglasPromocionTests`, **SS** `SolicitudServiceTests`, **CS** `ComisionServiceTests`, **ES** `EvidenciaServiceTests`, **GA** `GeneradorActoTests`, **AF** `AuditoriaFilterTests`, **E2E** `tests/e2e/api-smoke.sh`.

## Requisitos de promoción

| Disposición | Qué exige | Implementación | Verificación | Estado |
|-------------|-----------|----------------|--------------|--------|
| CES art. 63 | Escalafón de 3 categorías y 8 grados | `RRHH.Roles` (`Categoria`, `Nivel`, `Grado`, `PermanenciaAnios`); componente *Escalafón* | RP `Art63_Principal3_EsElUltimoGrado`; E2E "el escalafón tiene 8 roles" | ✅ |
| CES art. 76 (encabezado) | Promoción solo al grado inmediato; Agregado 3 → Principal 1 es por concurso | Regla **R1** (`Promocionable`); `SolicitudService.CrearAsync` rechaza con 400 | RP `Art76_Agregado3_NoTienePromocion_SeAccedePorConcurso`; SS `Art76_Agregado3_NoPuedeSolicitarPromocion`; E2E "Jorge no puede solicitar desde Agregado 3" | ✅ |
| CES art. 76 y UTA art. 48 (valores) | La IES fija los requisitos sobre los mínimos del CES | `DITIC.RequisitosPromocion` versionada; `RequisitoService` impide bajar de los mínimos y exige requisitos crecientes; pantalla *Requisitos* | E2E "no se aceptan requisitos por debajo del mínimo del CES" | 🟡 valores propuestos (D1, D2) hasta que la UTA expida su normativa |
| UTA transitoria cuarta | La UTA expedirá su normativa interna de promoción | Requisitos marcados `EsPropuesta`; aviso en el dictamen | Revisión visual del dictamen | 🟡 |
| CES transitoria novena | Se evalúa con la norma vigente al presentar la solicitud | `SolicitudesPromocion.RequisitosPromocionId`; el dictamen definitivo usa esa versión | SS `Art99_Crear_GuardaFotoDeRequisitosYDictamen`, `EnviarAComision_ConTodoRevisado_GuardaElDictamenDefinitivo` | ✅ |
| CES arts. 76 a y 77 a; UTA art. 48 a | Permanencia mínima en el grado (4 años; 3 en Principal 1 y 2) | Regla **R2** con fecha completa (`Fechas.MesesCompletos`) | RP `Art76a_PermanenciaMinimaSegunGrado`, `Art76a_CuentaConFechaCompleta_NoSoloElAnio` | ✅ |
| UTA transitoria quinta (experiencia) | Quien ingresó antes del 07-11-2012 acumula tiempo como no titular y técnico docente | **R2** con `RRHH.TrayectoriaDocente` | RP `TransitoriaQuinta_IngresoAntesDe2012_ExperienciaAcumulada`, `TransitoriaQuinta_NoAplicaAQuienIngresoDespuesDe2012`; E2E escenario de Marta | ✅ |
| CES art. 76 b; UTA art. 48 b | Obras de relevancia en la carrera, al menos una en el grado actual | Regla **R3** | RP `Art76b_ExigeAlMenosUnaObraEnElGradoActual`, `Art76b_CuentaObrasDeTodaLaCarrera_NoSoloDelGrado` | ✅ |
| CES arts. 78-87 | Criterios de obra de relevancia por tipo (artículo arbitrado e indexado, libro con pares e ISBN, registro SENADI…) | `Obra.EvaluarRelevancia()`; columna *Relevancia* con el motivo | RP `Art82_ArticuloSinCriteriosNoEsObraRelevante`, `Arts80a87_CriteriosPorTipoDeObra` | ✅ |
| CES art. 76 c; UTA art. 48 c | Promedio ≥ 75 % en las 4 últimas evaluaciones | Regla **R4**, excluye periodos sin docencia | RP `Art76c_UsaSoloLasCuatroUltimasEvaluaciones`, `Art76c_PromedioMenorA75_NoCumple`, `Art76c_ExcluyePeriodosSinDocencia`, `Art76c_MenosDeCuatroEvaluaciones_NoCumple` | ✅ |
| UTA-EV art. 15 | Menos de 70 % en dos periodos consecutivos es causal de destitución | Alerta en el dictamen (no bloquea la promoción) | RP `EvaluacionUtaArt15_AlertaPorDosPeriodosConsecutivosBajo70` | ✅ |
| CES art. 76 d; UTA art. 48 d | 96 horas en los últimos 3 años, 25 % pedagógicas o congresos internacionales | Regla **R5** | RP `Art76d_SoloCuentanLasHorasDeLosUltimosTresAnios`, `Art76d_Exige25PorCientoPedagogico`, `Art76d_LosCongresosInternacionalesCuentanComoPedagogicos` | ✅ |
| CES art. 77 e | Sabático: capacitación reducida 25 % | **R5** con `RRHH.Sabaticos` | RP `Art77e_Sabatico_Reduce25PorCiento` | ✅ |
| CES art. 77 f | Autoridades: exención de obra en el grado y capacitación; evaluación de los últimos 2 años | **R3**, **R4**, **R5** con `RRHH.CargosAutoridad` | RP `Art77f_AutoridadDosAniosOMas_ExentaDePublicarEnElGrado`, `Art77f_Autoridad_SeConsideranLosUltimosDosAniosEvaluados`, `Art76d_AutoridadMasDeDosAnios_Exenta` | ✅ |
| CES art. 77 g-i | Equivalencias en horas: evaluador externo, facilitador CES, par CACES | **R5** con `DEaDV.Equivalencias` | RP `Art77g_EvaluadorExterno_Equivale16Horas`, `Art77h_SoloUnaFacilitacionDelCESPorPromocion` | ✅ |
| UTA art. 48 e.1; CES art. 76 e.1 | 12 meses en proyectos de investigación o vinculación | Regla **R6**, ponderación de CES art. 36 c.2 | RP `UtaArt48e1_ProyectosMenosDe12Meses_NoCumple`, `UtaArt48e1_NoSeEvaluaSiLaUniversidadNoLoExige` | 🟡 activado por D2 |
| UTA art. 48 e.2; CES art. 76 e.2 | Dirección o codirección de proyectos | Regla **R7** | RP `UtaArt48e2_DireccionOCodireccionDeProyectos` | 🟡 activado por D2 |
| UTA art. 48 e.3; CES arts. 36 b.3 y 77 j | Dirección de trabajos de titulación; 1 doctorado = 3 maestrías; 3 evaluaciones CES = 1 dirección | Regla **R8** | RP `Art36_UnDoctoradoEquivaleATresMaestrias`, `UtaArt48e3_TesisDeGradoNoCuentanSiSeExigeMaestria`, `Art77j_TresEvaluacionesCES_EquivalenAUnaDireccion` | 🟡 activado por D2 |
| UTA transitoria quinta (PhD) | Quien ingresó con PhD: tesis de cualquier nivel son equivalentes | **R8** con `IngresoConPhD` | RP `TransitoriaQuinta_IngresoConPhD_TesisDeCualquierNivelSonEquivalentes` | ✅ |

## Procedimiento

| Disposición | Qué exige | Implementación | Verificación | Estado |
|-------------|-----------|----------------|--------------|--------|
| CES art. 99 (solicitud) | El profesor solicita tras cumplir la permanencia, anexando evidencias | `POST /api/solicitudes` rechaza sin permanencia; `POST …/evidencias` (solo PDF ≤ 10 MB) | SS `Art99_SinPermanenciaMinima_NoPuedeSolicitar`; ES `Art99_ElDocenteAnexaUnPdfYSeGuardaSuHash`, `SoloSeAceptanPdfReales_NoBastaLaExtension`, `ArchivoDeMasDe10MB_Da400`; E2E "Carlos no cumple la permanencia mínima…" | ✅ |
| CES art. 99 (verificación) | Talento Humano verifica el cumplimiento | Revisión documental de cada documento del expediente; no se envía a la Comisión con pendientes | SS `RevisarDocumento_*`, `EnviarAComision_ConDocumentosSinRevisar_Da409`; E2E sección "Revisión documental" | ✅ |
| CES art. 99 (presupuesto) | La promoción requiere disponibilidad presupuestaria | Voto del Director Financiero con `CertificaPresupuesto`; sin ella no se aprueba | SS `Art99_SinCertificacionPresupuestaria_NoSeAprueba`, `Art47_SoloElDirectorFinancieroCertificaPresupuesto` | ✅ |
| UTA art. 47; CES art. 75 | La Comisión Académica (Vicerrector Académico, profesor del HCU, dos de las ternas, Director Financiero, Director de Talento Humano) resuelve | `ComisionMiembros`, votación con quórum 4, mayoría simple y voto dirimente de la presidencia; pantalla *Comisión* | CS `Art47_*`; SS `Art47_*`; E2E sección "Comisión Académica" | 🟡 quórum y mayoría propuestos (D3) |
| Principio de imparcialidad (COA art. 19) | Nadie decide sobre su propio caso | Un miembro no vota su propia solicitud | SS `Art47_UnMiembroNoVotaSuPropiaSolicitud`; E2E "Lucía no vota su propia solicitud" | ✅ |
| COA art. 100 | Los actos administrativos se motivan | Motivación obligatoria en cada voto, en el cierre y en la resolución del HCU | SS `Art47_VotoSinMotivacion_Da400`, `COA100_CerrarSinMotivacion_Da400`, `ResolverApelacionSinNumeroDeResolucion_Da400` | ✅ |
| CES art. 77 l | La promoción consta en un acto con fecha | Resolución numerada en PDF con considerandos, verificación de requisitos y votos; huella SHA-256; registro de la acción de personal | SS `Art77l_*`; GA `GeneraUnPdfValidoConTablaDeReferenciasCorrecta`, `IncluyeNumeroDecisionArticulosYVotos`; E2E "Ana descarga su resolución" | ✅ (firma ⏳) |
| CES art. 77 l (firma) | El acto lo suscribe la autoridad | El PDF incluye los espacios de firma; la firma electrónica con FirmaEC queda fuera (D6) | — | ⏳ |
| COA arts. 217-218 | Apelación ante el órgano superior; decisión definitiva en vía administrativa | `POST …/apelacion`; el Administrador registra la resolución del HCU; una sola apelación | SS `COA217_*`, `COA218_SoloSeApelaUnaVez`, `ApelacionAceptada_ApruebaYEmiteNuevoActo`; GA `CuandoSeNiega_InformaElRecursoDeApelacion`; E2E sección "negativa y apelación" | 🟡 órgano y plazo propuestos (D4) |
| COA art. 224 | Plazo de 10 días para apelar | `Apelacion.PlazoHasta` (días hábiles) | SS `COA224_ElPlazoEsDeDiezDiasHabiles`, `COA224_ApelacionFueraDePlazo_Da409` | 🟡 confirmar con Procuraduría (D4) |

## Reglamento DITIC

| Disposición | Qué exige | Implementación | Verificación | Estado |
|-------------|-----------|----------------|--------------|--------|
| Art. 2 | Integridad, confidencialidad y disponibilidad de la información | Bitácora `Auditoria`; permisos mínimos sin `DELETE`; evidencias y auditoría solo `INSERT`; hashes SHA-256 | AF `NoGuardaContrasenas_YRegistraElIntentoDeLogin`, `LasConsultasNoSeRegistran`, `SiLaBitacoraFalla_LaOperacionContinua`; E2E "la bitácora registra los cierres de votación" | ✅ |
| Art. 18 c | Informes de funcionamiento y utilización | `GET /health`, `GET /api/reportes/uso`, pantalla *Operación* | E2E "las cinco bases responden", "reporte de utilización" | ✅ |
| Art. 23 e | Respaldos | `docker/backup-db.sh` (verificado con `RESTORE VERIFYONLY`), `docker/restore-db.sh`, retención | Paso de respaldo en CI; prueba manual de respaldo y restauración en [plan de pruebas](plan-pruebas.md) | ✅ |
| Art. 23 f | Pruebas de rendimiento y de seguridad | k6 (`tests/performance/carga.js`, workflow *Rendimiento*); OWASP ZAP en CI; límite de login; revocación inmediata | Workflows `rendimiento.yml` y `ci.yml` | ✅ |
| Arts. 18 a y 27 | Plan de mejoramiento y aseguramiento de la calidad | [Plan de mejoramiento](plan-mejoramiento.md), [plan de pruebas](plan-pruebas.md), CI con linters y pruebas | CI | ✅ |
| Contrato con otras direcciones | Las bases de RRHH, DIDE, DEaDV y DAC son de otras unidades | [Contrato de datos](contrato-datos-ditic.md) con las columnas que el sistema lee | — | 🟡 pendiente de acordar con cada dirección |

## Decisiones tomadas por defecto

Estas decisiones son de la universidad. Se implementaron con valores por defecto y se pueden cambiar sin tocar el código salvo donde se indica.

| # | Decisión | Valor aplicado | Dónde se cambia |
|---|----------|----------------|-----------------|
| D1 | Obras de relevancia por grado | 1, 2, 3, 4, 7, 8 desde Auxiliar 1 hasta Principal 2; obras de toda la carrera con al menos una en el grado | Pantalla *Requisitos* |
| D2 | Requisitos opcionales de la UTA (art. 48 e) | Activados: 12 meses en proyectos desde Auxiliar 2, dirección desde Agregado 2, trabajos de titulación desde Agregado 1 (maestría en Principal) | Pantalla *Requisitos* |
| D3 | Quórum y mayoría de la Comisión | Quórum 4 de 6, mayoría simple de votos emitidos, voto dirimente de la presidencia | `SolicitudService.Quorum` y `ResultadoVotacion` (código) |
| D4 | Órgano y plazo de la apelación | Consejo Universitario, 10 días hábiles | `Apelacion.PlazoHasta` (código) |
| D5 | Dónde se guardan las evidencias | Tabla `DITIC.EvidenciasSolicitud`, PDF de hasta 10 MB | `EvidenciaSolicitud.TamanoMaximo` (código) |
| D6 | Firma del acto | PDF generado por el sistema; FirmaEC fuera del sistema | — |

## Diferencias con el plan original

| Plan | Implementación | Motivo |
|------|----------------|--------|
| Tabla `HistorialEscalafon` en RRHH | Se usa `TiempoRol` para el grado actual y `TrayectoriaDocente` para la experiencia acumulada | La permanencia solo necesita el inicio del grado actual; el historial completo no cambia ninguna regla |
| Rol de sistema `ComisionAcademica` | Membresía en `ComisionMiembros` con vigencia | Los integrantes ya tienen otro rol (docente, Talento Humano, autoridad) y cambian con cada periodo |
| PDF con MigraDoc/PdfSharp | Generador propio mínimo (`DocumentoPdf`) | Evita una dependencia para un documento de texto; se valida la estructura en GA |
| Auditoría con valores antes/después | Se registran la acción, la ruta, el resultado y los argumentos | Los cambios de estado ya quedan en cada tabla con quién y cuándo; ver [plan de mejoramiento](plan-mejoramiento.md) |
