using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Accion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idAcciones {  get; set; }

        public int idRoles { get; set; }
        [ForeignKey("idRoles")]
        public virtual Roles Roles { get; set; }

        public int idPermiso { get; set; }
        [ForeignKey("idPermiso")]
        public virtual Permiso Permiso {  get; set; }

    }
}
