using ApiREST_UTA.Application.Services;

namespace ApiREST_UTA.Tests
{
    public class ClaimsUsuarioTests
    {
        [Fact]
        public void Revisores_PuedenConsultarCualquierCedula()
        {
            Assert.True(Usuarios.Rrhh().PuedeConsultar(Usuarios.CedulaCarlos));
            Assert.True(Usuarios.Administrador().PuedeConsultar(Usuarios.CedulaAna));
        }

        [Fact]
        public void Docente_SoloPuedeConsultarSuPropiaCedula()
        {
            var ana = Usuarios.Docente(cedula: Usuarios.CedulaAna);

            Assert.True(ana.PuedeConsultar(Usuarios.CedulaAna));
            Assert.True(ana.PuedeConsultar($" {Usuarios.CedulaAna} "));
            Assert.False(ana.PuedeConsultar(Usuarios.CedulaCarlos));
            Assert.False(ana.PuedeConsultar(null));
        }

        [Fact]
        public void DocenteSinCedula_NoPuedeConsultarNada()
        {
            Assert.False(Usuarios.Docente(cedula: null).PuedeConsultar(Usuarios.CedulaAna));
        }

        [Fact]
        public void Id_LeeElIdentificadorDelUsuario()
        {
            Assert.Equal(7, Usuarios.Docente(id: 7).Id());
        }
    }
}
