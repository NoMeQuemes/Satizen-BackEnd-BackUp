using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models.Dto.Acciones
{
    public class AccionesDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idAcciones { get; set; }
        public int idRoles { get; set; }
        public int idPermiso { get; set; }

    }
}
