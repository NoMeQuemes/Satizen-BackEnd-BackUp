using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Notificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idNotificacion { get; set; }

        public int? idUsuario { get; set; }
        [ForeignKey("idUsuario")]
        public virtual Usuario Usuario { get; set; }

        public string titulo { get; set; }
        public string contenido { get; set; }
        public string tipo { get; set; } //Puede ser: mensaje, llamado, asignación
        public DateTime? IsRead { get; set; } // Acá se verifica si la notificación fue vista
        public DateTime fechaCreacion { get; set; }

    }
}
