namespace Satizen_Api.Models.Dto.Usuarios
{
    public class UsuarioDto
    {
        public int idUsuario { get; set; }
        public string nombreUsuario { get; set; }
        public string password { get; set; }
        public IFormFile imagenPefil { get; set; }
        public string imagenPerfilUrl { get; set; }
        public int idRoles { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime fechaActualizacion { get; set; }
        public DateTime fechaEliminacion { get; set; }

    }
}
