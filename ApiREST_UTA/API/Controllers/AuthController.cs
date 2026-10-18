using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiREST_UTA.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting(Program.PoliticaLimiteLogin)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.AutenticarAsync(request);
            if (result == null)
                return Unauthorized(new { mensaje = "Credenciales inválidas" });

            return Ok(result);
        }
    }
}
