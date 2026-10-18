using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Infrastructure.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ApiREST_UTA
{
    public class Program
    {
        private const string PoliticaCorsFrontend = "Frontend";

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
            builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
            builder.Services.AddScoped<IRequisitoRepository, RequisitoRepository>();
            builder.Services.AddScoped<IEquivalenciaRepository, EquivalenciaRepository>();

            // Inyección de servicios
            builder.Services.AddScoped<ObraService>();
            builder.Services.AddScoped<CursoService>();
            builder.Services.AddScoped<InvestigacionService>();
            builder.Services.AddScoped<PromocionService>();
            builder.Services.AddScoped<TesisService>();
            builder.Services.AddScoped<ObraIdiomaService>();
            builder.Services.AddScoped<ProyectoPonderadoService>();
            builder.Services.AddScoped<CapacitacionImpartidaService>();
            builder.Services.AddScoped<SolicitudService>();
            builder.Services.AddScoped<UsuarioService>();
            builder.Services.AddScoped<RequisitoService>();
            builder.Services.AddScoped<AuthService>(sp =>
                new AuthService(sp.GetRequiredService<IUsuarioRepository>(), jwtKey));

            builder.Services.AddControllers();

            // CORS: orígenes permitidos separados por coma en CORS_ORIGINS (por defecto el frontend de Vite)
            var origenesPermitidos = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:5173")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(PoliticaCorsFrontend, policy => policy
                    .WithOrigins(origenesPermitidos)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

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
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Promoción Docente UTA",
                    Version = "v1",
                    Description = "Importa la información del docente desde DITIC, RRHH, DIDE, DEaDV y DAC y valida los requisitos de promoción en el escalafón."
                });

                // Botón "Authorize": pega el token devuelto por POST /api/Auth/login
                var esquemaJwt = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Token JWT obtenido en /api/Auth/login (sin el prefijo 'Bearer').",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };
                options.AddSecurityDefinition("Bearer", esquemaJwt);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquemaJwt] = Array.Empty<string>() });
            });

            var app = builder.Build();

            // Swagger en Development, o en cualquier entorno con SWAGGER_ENABLED=true
            if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("SWAGGER_ENABLED"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Promoción Docente UTA v1");
                    options.DocumentTitle = "API Promoción Docente UTA";
                });
            }

            app.UseCors(PoliticaCorsFrontend);
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
