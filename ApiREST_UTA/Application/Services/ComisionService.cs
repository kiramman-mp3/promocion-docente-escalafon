using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;

namespace ApiREST_UTA.Application.Services
{
    // Conformación de la Comisión Académica de promoción (UTA 0153-HCU-2026, art. 47)
    public class ComisionService
    {
        private readonly IComisionRepository _comision;
        private readonly IUsuarioRepository _usuarios;

        public ComisionService(IComisionRepository comision, IUsuarioRepository usuarios)
        {
            _comision = comision;
            _usuarios = usuarios;
        }

        public Task<List<MiembroComision>> ListarAsync(bool incluirHistorico) => _comision.ListarMiembrosAsync(incluirHistorico);

        public async Task<MiembroComision> AgregarAsync(MiembroComisionRequest request)
        {
            if (!MiembroComision.Integrantes.ContainsKey(request.Cargo))
                throw ReglaNegocioException.Invalido($"Cargo inválido. Usa: {string.Join(", ", MiembroComision.Integrantes.Keys)}.");

            var usuario = await _usuarios.ObtenerPorIdAsync(request.UsuarioId)
                ?? throw ReglaNegocioException.NoEncontrado("El usuario no existe.");
            if (!usuario.Activo)
                throw ReglaNegocioException.Invalido("El usuario está desactivado.");

            var activos = await _comision.ListarMiembrosAsync(incluirHistorico: false);
            if (activos.Any(m => m.UsuarioId == request.UsuarioId))
                throw ReglaNegocioException.Conflicto("El usuario ya integra la Comisión.");
            if (activos.Count(m => m.Cargo == request.Cargo) >= MiembroComision.Integrantes[request.Cargo])
                throw ReglaNegocioException.Conflicto("Ese cargo ya está ocupado: finaliza primero la membresía actual (UTA art. 47).");

            // Art. 47: preside el Vicerrector Académico o su delegado
            if (request.EsPresidente && request.Cargo != MiembroComision.VicerrectorAcademico)
                throw ReglaNegocioException.Invalido("La Comisión la preside el Vicerrector Académico o su delegado (UTA art. 47).");
            if (request.EsPresidente && activos.Any(m => m.EsPresidente))
                throw ReglaNegocioException.Conflicto("Ya hay una presidencia vigente.");

            var miembro = new MiembroComision
            {
                UsuarioId = request.UsuarioId,
                Cargo = request.Cargo,
                EsPresidente = request.EsPresidente,
                VigenteDesde = (request.VigenteDesde ?? DateTime.Today).Date
            };
            miembro.Id = await _comision.AgregarMiembroAsync(miembro);
            return (await _comision.ObtenerMiembroAsync(miembro.Id))!;
        }

        public async Task FinalizarAsync(int id)
        {
            var miembro = await _comision.ObtenerMiembroAsync(id)
                ?? throw ReglaNegocioException.NoEncontrado("La membresía no existe.");
            if (miembro.VigenteHasta != null)
                throw ReglaNegocioException.Conflicto("La membresía ya estaba finalizada.");

            await _comision.FinalizarMiembroAsync(id, DateTime.Today);
        }
    }
}
