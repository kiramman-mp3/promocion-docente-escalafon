using ApiREST_UTA.Application.Services.Promocion;
using ApiREST_UTA.Domain.Entities;
using static ApiREST_UTA.Tests.Promocion.Escenario;

namespace ApiREST_UTA.Tests.Promocion
{
    // Cada prueba nombra el artículo que verifica (CES RPC-SE-19-No.055-2021; UTA 0153-HCU-2026)
    public class ReglasPromocionTests
    {
        [Fact]
        public void EscenarioBase_CumpleTodosLosRequisitos()
        {
            var dictamen = new MotorPromocion().Evaluar(Base());

            Assert.True(dictamen.CumpleTodo, string.Join(" | ", dictamen.Requisitos.Where(r => !r.Cumple).Select(r => $"{r.Codigo}: {r.Obtenido}")));
            Assert.Equal("Agregado 2", dictamen.RolObjetivo);
        }

        // ---------- R1: grado con promoción ----------

        [Fact]
        public void Art76_Agregado3_NoTienePromocion_SeAccedePorConcurso()
        {
            var dictamen = new MotorPromocion().Evaluar(Base(gradoActual: 5));

            Assert.False(dictamen.Promocionable);
            Assert.False(dictamen.CumpleTodo);
            Assert.Single(dictamen.Requisitos);
            Assert.Contains("concurso", dictamen.Requisitos[0].Detalle);
        }

        [Fact]
        public void Art63_Principal3_EsElUltimoGrado()
        {
            var dictamen = new MotorPromocion().Evaluar(Base(gradoActual: 8));

            Assert.False(dictamen.Promocionable);
            Assert.Null(dictamen.RolObjetivo);
        }

        [Fact]
        public void Art77_SinRequisitosVigentes_NoCumple()
        {
            var c = Base();
            c.Requisitos = null;

            var dictamen = new MotorPromocion().Evaluar(c);

            Assert.False(dictamen.CumpleTodo);
            Assert.Contains(dictamen.Requisitos, r => r.Codigo == "R0");
        }

        // ---------- R2: permanencia ----------

        [Theory]
        [InlineData(3, 47, false)] // grados 1-4: 4 años
        [InlineData(3, 48, true)]
        [InlineData(6, 35, false)] // grados 6-7: 3 años
        [InlineData(6, 36, true)]
        [InlineData(7, 36, true)]
        public void Art76a_PermanenciaMinimaSegunGrado(int grado, int mesesEnGrado, bool cumple)
        {
            var c = Base(grado);
            c.FechaInicioGrado = Hoy.AddMonths(-mesesEnGrado);

            Assert.Equal(cumple, Resultado(c, ReglaPermanencia.Codigo).Cumple);
        }

        [Fact]
        public void Art76a_CuentaConFechaCompleta_NoSoloElAnio()
        {
            var c = Base();
            // Empezó en diciembre de hace 4 años según el calendario, pero aún no completa 4 años
            c.FechaInicioGrado = new DateTime(Hoy.Year - 4, 12, 1);

            Assert.False(Resultado(c, ReglaPermanencia.Codigo).Cumple);
        }

        [Fact]
        public void TransitoriaQuinta_IngresoAntesDe2012_ExperienciaAcumulada()
        {
            var c = Base(gradoActual: 4);
            c.FechaInicioGrado = Hoy.AddMonths(-28);
            c.Trayectoria = new TrayectoriaDocente
            {
                Cedula = "1701111111",
                FechaIngresoInstitucion = new DateTime(2006, 3, 1),
                FechaIngresoTitular = new DateTime(2008, 5, 1),
                MesesNoTitular = 24
            };

            var r = Resultado(c, ReglaPermanencia.Codigo);

            Assert.True(r.Cumple);
            Assert.True(r.Exento);
            Assert.Contains("transitoria quinta", r.Detalle);
        }

        [Fact]
        public void TransitoriaQuinta_NoAplicaAQuienIngresoDespuesDe2012()
        {
            var c = Base(gradoActual: 4);
            c.FechaInicioGrado = Hoy.AddMonths(-28);
            c.Trayectoria!.FechaIngresoInstitucion = new DateTime(2013, 1, 1);
            c.Trayectoria.FechaIngresoTitular = new DateTime(2013, 1, 1);

            Assert.False(Resultado(c, ReglaPermanencia.Codigo).Cumple);
        }

        // ---------- R3: obras de relevancia ----------

        [Fact]
        public void Art76b_ExigeAlMenosUnaObraEnElGradoActual()
        {
            var c = Base();
            c.Obras = new() { ObraRelevante(Hoy.AddYears(-9)), ObraRelevante(Hoy.AddYears(-8)), ObraRelevante(Hoy.AddYears(-7)) };

            var r = Resultado(c, ReglaObras.Codigo);

            Assert.False(r.Cumple);
            Assert.Contains("0 en el grado actual", r.Obtenido);
        }

        [Fact]
        public void Art76b_CuentaObrasDeTodaLaCarrera_NoSoloDelGrado()
        {
            var c = Base();
            // Dos obras antes del grado y una durante: 3 en total cumple
            c.Obras = new() { ObraRelevante(Hoy.AddYears(-9)), ObraRelevante(Hoy.AddYears(-8)), ObraRelevante(Hoy.AddYears(-1)) };

            Assert.True(Resultado(c, ReglaObras.Codigo).Cumple);
        }

        [Theory]
        [InlineData(false, "1390-0001", "Scopus", "revisión por pares")]
        [InlineData(true, null, "Scopus", "ISSN")]
        [InlineData(true, "1390-0001", null, "índice")]
        public void Art82_ArticuloSinCriteriosNoEsObraRelevante(bool pares, string? issn, string? indice, string motivo)
        {
            var obra = ObraRelevante(Hoy);
            obra.RevisionPares = pares;
            obra.ISSN = issn;
            obra.BaseIndexacion = indice;

            var (relevante, explicacion) = obra.EvaluarRelevancia();

            Assert.False(relevante);
            Assert.Contains(motivo, explicacion);
        }

        [Theory]
        [InlineData(Obra.Libro, true, "978-9942", null, null, true)]
        [InlineData(Obra.CapituloLibro, false, "978-9942", null, null, false)]
        [InlineData(Obra.PropiedadIndustrial, false, null, "SENADI-1", null, true)]
        [InlineData(Obra.DisenoSoftwarePrototipo, false, null, null, null, false)]
        [InlineData(Obra.Ponencia, true, null, null, "Scopus", true)]
        [InlineData(Obra.Otra, true, "978-9942", null, "Scopus", false)]
        public void Arts80a87_CriteriosPorTipoDeObra(string tipo, bool pares, string? isbn, string? senadi, string? indice, bool relevante)
        {
            var obra = new Obra
            {
                Cedula = "1",
                NombreRevista = "x",
                TipoObra = tipo,
                RevisionPares = pares,
                ISBN = isbn,
                RegistroSENADI = senadi,
                BaseIndexacion = indice
            };

            Assert.Equal(relevante, obra.EvaluarRelevancia().Relevante);
        }

        [Fact]
        public void Art77f_AutoridadDosAniosOMas_ExentaDePublicarEnElGrado()
        {
            var c = Base();
            c.Obras = new() { ObraRelevante(Hoy.AddYears(-9)), ObraRelevante(Hoy.AddYears(-8)), ObraRelevante(Hoy.AddYears(-7)) };
            c.CargosAutoridad.Add(new CargoAutoridad
            {
                Cedula = c.Cedula,
                Cargo = "Vicerrectora Académica",
                TipoCargo = CargoAutoridad.Vicerrector,
                FechaInicio = Hoy.AddMonths(-30),
                FechaFin = Hoy.AddMonths(-2)
            });

            var r = Resultado(c, ReglaObras.Codigo);

            Assert.True(r.Cumple);
            Assert.True(r.Exento);
        }

        // ---------- R4: evaluación de desempeño ----------

        [Fact]
        public void Art76c_UsaSoloLasCuatroUltimasEvaluaciones()
        {
            var c = Base();
            // Una evaluación antigua baja no debe afectar si las 4 últimas superan el 75 %
            c.Evaluaciones.Add(Evaluacion(Hoy.AddYears(-6), 40));

            var r = Resultado(c, ReglaEvaluacion.Codigo);

            Assert.True(r.Cumple);
            Assert.Contains("4 periodos", r.Obtenido);
        }

        [Fact]
        public void Art76c_PromedioMenorA75_NoCumple()
        {
            var c = Base();
            c.Evaluaciones = new()
            {
                Evaluacion(Hoy.AddMonths(-30), 70), Evaluacion(Hoy.AddMonths(-24), 72),
                Evaluacion(Hoy.AddMonths(-18), 76), Evaluacion(Hoy.AddMonths(-12), 80)
            };

            Assert.False(Resultado(c, ReglaEvaluacion.Codigo).Cumple);
        }

        [Fact]
        public void Art76c_ExcluyePeriodosSinDocencia()
        {
            var c = Base();
            c.Evaluaciones.Add(Evaluacion(Hoy.AddMonths(-6), 10, docencia: false));

            var r = Resultado(c, ReglaEvaluacion.Codigo);

            Assert.True(r.Cumple);
            Assert.Contains("sin docencia", r.Detalle);
        }

        [Fact]
        public void Art76c_MenosDeCuatroEvaluaciones_NoCumple()
        {
            var c = Base();
            c.Evaluaciones.RemoveAt(0);

            Assert.False(Resultado(c, ReglaEvaluacion.Codigo).Cumple);
        }

        [Fact]
        public void Art77f_Autoridad_SeConsideranLosUltimosDosAniosEvaluados()
        {
            var c = Base();
            c.Evaluaciones = new() { Evaluacion(Hoy.AddYears(-6), 50), Evaluacion(Hoy.AddMonths(-6), 95, docencia: false) };
            c.CargosAutoridad.Add(new CargoAutoridad
            {
                Cedula = c.Cedula,
                Cargo = "Rector",
                TipoCargo = CargoAutoridad.Rector,
                FechaInicio = Hoy.AddMonths(-20),
                FechaFin = null
            });

            var r = Resultado(c, ReglaEvaluacion.Codigo);

            Assert.True(r.Cumple);
            Assert.True(r.Exento);
        }

        // ---------- R5: capacitación ----------

        [Fact]
        public void Art76d_SoloCuentanLasHorasDeLosUltimosTresAnios()
        {
            var c = Base();
            c.Cursos = new() { Curso(6, 40, CursoCapacitacion.Pedagogica), Curso(40, 200) };

            var r = Resultado(c, ReglaCapacitacion.Codigo);

            Assert.False(r.Cumple);
            Assert.StartsWith("40 h", r.Obtenido);
        }

        [Fact]
        public void Art76d_Exige25PorCientoPedagogico()
        {
            var c = Base();
            c.Cursos = new() { Curso(6, 20, CursoCapacitacion.Pedagogica), Curso(10, 100) };

            Assert.False(Resultado(c, ReglaCapacitacion.Codigo).Cumple);
        }

        [Fact]
        public void Art76d_LosCongresosInternacionalesCuentanComoPedagogicos()
        {
            var c = Base();
            c.Cursos = new() { Curso(6, 24, CursoCapacitacion.CongresoInternacional), Curso(10, 72) };

            Assert.True(Resultado(c, ReglaCapacitacion.Codigo).Cumple);
        }

        [Fact]
        public void Art76d_AutoridadMasDeDosAnios_Exenta()
        {
            var c = Base();
            c.Cursos.Clear();
            c.CargosAutoridad.Add(new CargoAutoridad
            {
                Cedula = c.Cedula,
                Cargo = "Decano",
                TipoCargo = CargoAutoridad.AutoridadAcademica,
                FechaInicio = Hoy.AddMonths(-30),
                FechaFin = Hoy.AddMonths(-1)
            });

            var r = Resultado(c, ReglaCapacitacion.Codigo);

            Assert.True(r.Cumple);
            Assert.True(r.Exento);
        }

        [Fact]
        public void Art77e_Sabatico_Reduce25PorCiento()
        {
            var c = Base();
            c.Cursos = new() { Curso(6, 40, CursoCapacitacion.Pedagogica), Curso(10, 32) }; // 72 h = 75 % de 96
            c.Sabaticos.Add(new Sabatico
            {
                Cedula = c.Cedula,
                FechaInicio = Hoy.AddMonths(-20),
                FechaFin = Hoy.AddMonths(-8),
                InformeAprobado = true
            });

            var r = Resultado(c, ReglaCapacitacion.Codigo);

            Assert.True(r.Cumple);
            Assert.StartsWith("72 h", r.Requerido);
        }

        [Fact]
        public void Art77g_EvaluadorExterno_Equivale16Horas()
        {
            var c = Base();
            c.Cursos = new() { Curso(6, 40, CursoCapacitacion.Pedagogica), Curso(10, 40) }; // 80 h
            c.Equivalencias.Add(new Equivalencia { Cedula = c.Cedula, Tipo = Equivalencia.EvaluadorExternoConcurso, Fecha = Hoy.AddMonths(-3), Descripcion = "x" });

            Assert.True(Resultado(c, ReglaCapacitacion.Codigo).Cumple);
        }

        [Fact]
        public void Art77h_SoloUnaFacilitacionDelCESPorPromocion()
        {
            var c = Base();
            c.Cursos = new() { Curso(6, 30, CursoCapacitacion.Pedagogica), Curso(10, 30) }; // 60 h
            c.Equivalencias.Add(new Equivalencia { Cedula = c.Cedula, Tipo = Equivalencia.FacilitadorCESGradoMaestria, Fecha = Hoy.AddMonths(-3), Descripcion = "x" });
            c.Equivalencias.Add(new Equivalencia { Cedula = c.Cedula, Tipo = Equivalencia.FacilitadorCESDoctorado, Fecha = Hoy.AddMonths(-4), Descripcion = "y" });

            // 60 + 32 (solo la mayor) = 92 < 96
            Assert.False(Resultado(c, ReglaCapacitacion.Codigo).Cumple);
        }

        // ---------- R6-R8: requisitos opcionales de la UTA ----------

        [Fact]
        public void UtaArt48e1_ProyectosMenosDe12Meses_NoCumple()
        {
            var c = Base();
            c.Proyectos[0].Meses = 11;

            Assert.False(Resultado(c, ReglaProyectos.Codigo).Cumple);
        }

        [Fact]
        public void UtaArt48e1_NoSeEvaluaSiLaUniversidadNoLoExige()
        {
            var c = Base();
            c.Requisitos!.ProyectosMeses = 0;

            Assert.DoesNotContain(new MotorPromocion().Evaluar(c).Requisitos, r => r.Codigo == ReglaProyectos.Codigo);
        }

        [Theory]
        [InlineData(ProyectoParticipacion.Participante, false)]
        [InlineData(ProyectoParticipacion.Codirector, true)]
        [InlineData(ProyectoParticipacion.Director, true)]
        public void UtaArt48e2_DireccionOCodireccionDeProyectos(string rol, bool cumple)
        {
            var c = Base();
            c.Requisitos!.RequiereDireccionProyecto = true;
            c.Proyectos[0].Rol = rol;

            Assert.Equal(cumple, Resultado(c, ReglaDireccionProyectos.Codigo).Cumple);
        }

        [Fact]
        public void Art36_UnDoctoradoEquivaleATresMaestrias()
        {
            var c = Base(gradoActual: 6);
            c.Requisitos!.TrabajosTitulacion = 3;
            c.Requisitos.TitulacionNivelMinimo = RequisitoPromocion.NivelMaestria;
            c.TrabajosTitulacion = new()
            {
                new TrabajoTitulacion { Cedula = c.Cedula, Titulo = "PhD", Nivel = RequisitoPromocion.NivelDoctorado, Universidad = "UTA", FechaAprobacion = Hoy }
            };

            Assert.True(Resultado(c, ReglaTitulacion.Codigo).Cumple);
        }

        [Fact]
        public void UtaArt48e3_TesisDeGradoNoCuentanSiSeExigeMaestria()
        {
            var c = Base(gradoActual: 6);
            c.Requisitos!.TrabajosTitulacion = 1;
            c.Requisitos.TitulacionNivelMinimo = RequisitoPromocion.NivelMaestria;

            Assert.False(Resultado(c, ReglaTitulacion.Codigo).Cumple);
        }

        [Fact]
        public void TransitoriaQuinta_IngresoConPhD_TesisDeCualquierNivelSonEquivalentes()
        {
            var c = Base(gradoActual: 6);
            c.Requisitos!.TrabajosTitulacion = 1;
            c.Requisitos.TitulacionNivelMinimo = RequisitoPromocion.NivelMaestria;
            c.Trayectoria = new TrayectoriaDocente
            {
                Cedula = c.Cedula,
                FechaIngresoInstitucion = new DateTime(2010, 1, 1),
                FechaIngresoTitular = new DateTime(2010, 1, 1),
                IngresoConPhD = true
            };

            Assert.True(Resultado(c, ReglaTitulacion.Codigo).Cumple);
        }

        [Fact]
        public void Art77j_TresEvaluacionesCES_EquivalenAUnaDireccion()
        {
            var c = Base();
            c.TrabajosTitulacion.Clear();
            for (var i = 0; i < 3; i++)
                c.Equivalencias.Add(new Equivalencia { Cedula = c.Cedula, Tipo = Equivalencia.EvaluacionAcademicaCES, Fecha = Hoy.AddMonths(-i), Descripcion = "x" });

            var r = Resultado(c, ReglaTitulacion.Codigo);

            Assert.True(r.Cumple);
            Assert.Contains("77 j", r.Detalle);
        }

        // ---------- Alertas del reglamento de evaluación ----------

        [Fact]
        public void EvaluacionUtaArt15_AlertaPorDosPeriodosConsecutivosBajo70()
        {
            var alertas = MotorPromocion.AlertasEvaluacion(new()
            {
                Evaluacion(Hoy.AddMonths(-12), 65),
                Evaluacion(Hoy.AddMonths(-6), 68)
            });

            Assert.Contains(alertas, a => a.Contains("consecutivos"));
        }
    }
}
