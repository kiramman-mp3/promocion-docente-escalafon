using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public Usuario? GetByCorreo(string correo)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT * FROM Usuarios WHERE Correo = @correo", connection);
            command.Parameters.AddWithValue("@correo", correo);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Usuario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Correo = reader["Correo"].ToString()!,
                    Contrasena = reader["Contrasena"].ToString()!,
                    Nombre = reader["Nombre"].ToString()!,
                    Apellido = reader["Apellido"].ToString()!,
                    Rol = reader["Rol"].ToString()!,
                    Cedula = reader["Cedula"] == DBNull.Value ? null : reader["Cedula"].ToString(),
                    Activo = Convert.ToBoolean(reader["Activo"])
                };
            }
            return null;
        }
    }
}
