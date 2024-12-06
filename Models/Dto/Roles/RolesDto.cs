using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models.Dto.Roles
{
    public class RolesDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRol {  get; set; }
        public string nombre { get; set; }
        public string? descripcion { get; set; }


    }
}
