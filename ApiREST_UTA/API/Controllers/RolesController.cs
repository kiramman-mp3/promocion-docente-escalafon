using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRolRepository _rolRepo;

        public RolesController(IRolRepository rolRepo)
        {
            _rolRepo = rolRepo;
        }

        // Catálogo del escalafón con los requisitos de cada rol
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await _rolRepo.ListarAsync());
        }
    }
}
