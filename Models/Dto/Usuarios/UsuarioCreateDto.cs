using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Usuarios
{
    public class UsuarioCreateDto
    {
        [Required]
        public string nombreUsuario { get; set; }
        [Required]
        public string correo {  get; set; }
        [Required]
        public string password { get; set; }
        public int celularUsuario { get; set; }

        public IFormFile imagenPefil { get; set; }
        [Required]

        public int idRoles { get; set; }

    }
}
