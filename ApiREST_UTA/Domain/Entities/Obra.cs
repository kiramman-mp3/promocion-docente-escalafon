namespace ApiREST_UTA.Domain.Entities
{
    public class Obra
    {
        public string Cedula { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string NombreRevista { get; set; }
        public byte[] Pdf { get; set; }
    }
}
