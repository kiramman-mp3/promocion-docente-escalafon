using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ApiREST_UTA.Tests
{
    // Conformación de la Comisión Académica (UTA 0153-HCU-2026, art. 47)
    public class ComisionServiceTests
    {
        private readonly Mock<IComisionRepository> _comision = new();
        private readonly Mock<IUsuarioRepository> _usuarios = new();

        public ComisionServiceTests()
        {
            _usuarios.Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => new Usuario { Id = id, Activo = true });
            _comision.Setup(r => r.ListarMiembrosAsync(false)).ReturnsAsync(new List<MiembroComision>
            {
                new() { Id = 1, UsuarioId = 10, Cargo = MiembroComision.VicerrectorAcademico, EsPresidente = true },
                new() { Id = 2, UsuarioId = 11, Cargo = MiembroComision.ProfesorTerna }
            });
            _comision.Setup(r => r.AgregarMiembroAsync(It.IsAny<MiembroComision>())).ReturnsAsync(9);
            _comision.Setup(r => r.ObtenerMiembroAsync(9)).ReturnsAsync(new MiembroComision { Id = 9 });
        }

        private ComisionService Servicio() => new(_comision.Object, _usuarios.Object);

        private static async Task<int> CodigoDeError(Func<Task> accion) =>
            (await Assert.ThrowsAsync<ReglaNegocioException>(accion)).StatusCode;

        [Fact]
        public async Task Art47_SoloElVicerrectorAcademicoPreside()
        {
            var request = new MiembroComisionRequest { UsuarioId = 20, Cargo = MiembroComision.ProfesorHCU, EsPresidente = true };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().AgregarAsync(request)));
        }

        [Fact]
        public async Task Art47_LasTernasAportanDosProfesores()
        {
            var request = new MiembroComisionRequest { UsuarioId = 20, Cargo = MiembroComision.ProfesorTerna };

            var miembro = await Servicio().AgregarAsync(request);

            Assert.Equal(9, miembro.Id);
        }

        [Fact]
        public async Task Art47_CargoUnipersonalOcupado_Da409()
        {
            var request = new MiembroComisionRequest { UsuarioId = 20, Cargo = MiembroComision.VicerrectorAcademico };

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().AgregarAsync(request)));
        }

        [Fact]
        public async Task UnUsuarioNoIntegraDosVecesLaComision()
        {
            var request = new MiembroComisionRequest { UsuarioId = 11, Cargo = MiembroComision.DirectorFinanciero };

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().AgregarAsync(request)));
        }

        [Fact]
        public async Task CargoInexistente_Da400()
        {
            var request = new MiembroComisionRequest { UsuarioId = 20, Cargo = "Rector" };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().AgregarAsync(request)));
        }
    }
}
