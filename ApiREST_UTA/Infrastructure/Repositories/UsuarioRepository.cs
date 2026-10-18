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
            return reader.Read() ? LeerUsuario(reader) : null;
        }

        public async Task<List<Usuario>> ListarAsync()
        {
            var usuarios = new List<Usuario>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT * FROM Usuarios ORDER BY Rol, Apellido, Nombre", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                usuarios.Add(LeerUsuario(reader));

            return usuarios;
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT * FROM Usuarios WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? LeerUsuario(reader) : null;
        }

        public async Task<Usuario?> ObtenerPorCedulaAsync(string cedula)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT TOP 1 * FROM Usuarios WHERE Cedula = @cedula ORDER BY Activo DESC, Id", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? LeerUsuario(reader) : null;
        }

        public async Task<int> CrearAsync(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                INSERT INTO Usuarios (Correo, Contrasena, Nombre, Apellido, Rol, Cedula, Activo)
                OUTPUT INSERTED.Id
                VALUES (@correo, @contrasena, @nombre, @apellido, @rol, @cedula, @activo)", connection);
            command.Parameters.AddWithValue("@correo", usuario.Correo);
            command.Parameters.AddWithValue("@contrasena", usuario.Contrasena);
            AgregarDatosEditables(command, usuario);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                UPDATE Usuarios
                SET Nombre = @nombre, Apellido = @apellido, Rol = @rol, Cedula = @cedula, Activo = @activo
                WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", usuario.Id);
            AgregarDatosEditables(command, usuario);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task CambiarContrasenaAsync(int id, string contrasenaHash)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("UPDATE Usuarios SET Contrasena = @contrasena WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@contrasena", contrasenaHash);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private static void AgregarDatosEditables(SqlCommand command, Usuario usuario)
        {
            command.Parameters.AddWithValue("@nombre", usuario.Nombre);
            command.Parameters.AddWithValue("@apellido", usuario.Apellido);
            command.Parameters.AddWithValue("@rol", usuario.Rol);
            command.Parameters.AddWithValue("@cedula", (object?)usuario.Cedula ?? DBNull.Value);
            command.Parameters.AddWithValue("@activo", usuario.Activo);
        }

        private static Usuario LeerUsuario(SqlDataReader reader)
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
    }
}
