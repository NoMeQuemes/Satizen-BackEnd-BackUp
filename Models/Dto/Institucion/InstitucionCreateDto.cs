using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Institucion
{
    public class InstitucionCreateDto
    {

        [Required]
        [MaxLength(30)]
        public string? nombreInstitucion { get; set; }
        [Required]
        public string? direccionInstitucion { get; set; }
        [Required]
        public string? telefonoInstitucion { get; set; }
        [Required]
        public string? correoInstitucion { get; set; }
        [Required]
        public string? celularInstitucion { get; set; }
    }
}