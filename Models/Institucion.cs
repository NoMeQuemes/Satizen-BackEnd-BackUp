using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models
{
    public class Institucion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idInstitucion { get; set; }
        [Required]
        public string? nombreInstitucion { get; set; }
        [Required]
        public string? direccionInstitucion { get; set; }
        [Required]
        public string? telefonoInstitucion { get; set; }
        [Required]
        public string? correoInstitucion { get; set; }
        [Required]
        public string? celularInstitucion { get; set; }
        public DateTime? eliminado { get; set; }
    }
   
}

