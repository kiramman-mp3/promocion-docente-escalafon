using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Domain.Interfaces.ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Infrastructure.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiREST_UTA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Carga el .env de la raíz del repo (si existe) sin pisar variables ya definidas, p. ej. por docker
            Env.NoClobber().TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            // Cadenas de conexión armadas desde variables de entorno para no guardar secretos en appsettings.json
            var dbHost = builder.Configuration["DB_HOST"] ?? "localhost";
            var dbPort = builder.Configuration["DB_PORT"] ?? builder.Configuration["SQL_PORT"] ?? "1433";
            var dbUser = builder.Configuration["APP_DB_USER"];
            var dbPassword = builder.Configuration["APP_DB_PASSWORD"];
            var basesDeDatos = new Dictionary<string, string>
            {
                ["DITICConnection"] = "DITIC",
                ["RRHHConnection"] = "RRHH",
                ["DIDEConnection"] = "DIDE",
                ["DEaDVConnection"] = "DEaDV",
                ["DACConnection"] = "DAC"
            };
            foreach (var (nombreConexion, baseDeDatos) in basesDeDatos)
            {
                if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(nombreConexion)))
                    continue;
                if (string.IsNullOrWhiteSpace(dbUser) || string.IsNullOrWhiteSpace(dbPassword))
                    throw new Exception("Define APP_DB_USER y APP_DB_PASSWORD en el archivo .env");

                builder.Configuration[$"ConnectionStrings:{nombreConexion}"] =
                    $"Server={dbHost},{dbPort};Database={baseDeDatos};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
            }

            // JWT key desde la variable de entorno JWT_KEY (o Jwt:Key)
            var jwtKey = builder.Configuration["JWT_KEY"] ?? builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
                throw new Exception("Define JWT_KEY en el archivo .env (mínimo 32 caracteres)");

            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            // Inyección de dependencias de repositorios
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IRRHHRepository, RRHHRepository>();
            builder.Services.AddScoped<IObraRepository, ObrasRepository>();
            builder.Services.AddScoped<ICursoRepository, CursoRepository>();
            builder.Services.AddScoped<IInvestigacionRepository, InvestigacionRepository>();
            builder.Services.AddScoped<IEvaluacionDocenteRepository, EvaluacionDocenteRepository>();
            builder.Services.AddScoped<IRolRepository, RolRepository>();
            builder.Services.AddScoped<ITesisRepository, TesisRepository>();
            builder.Services.AddScoped<IObraIdiomaRepository, ObraIdiomaRepository>();
            builder.Services.AddScoped<IProyectoPonderadoRepository, ProyectoPonderadoRepository>();
            builder.Services.AddScoped<ICapacitacionImpartidaRepository, CapacitacionImpartidaRepository>();

            // Inyección de servicios
            builder.Services.AddScoped<ObraService>();
            builder.Services.AddScoped<CursoService>();
            builder.Services.AddScoped<InvestigacionService>();
            builder.Services.AddScoped<PromocionService>();
            builder.Services.AddScoped<TesisService>();
            builder.Services.AddScoped<ObraIdiomaService>();
            builder.Services.AddScoped<ProyectoPonderadoService>();
            builder.Services.AddScoped<CapacitacionImpartidaService>();
            builder.Services.AddScoped<AuthService>(sp =>
                new AuthService(sp.GetRequiredService<IUsuarioRepository>(), jwtKey));

            builder.Services.AddControllers();

            // Configuración de JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "ApiREST_UTA",
                        ValidateAudience = true,
                        ValidAudience = "ApiREST_UTA",
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey
                    };
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
