using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satizen_Api.Models.Dto.ContactoPaciente 
{ 
    public class UpdateContactoPacienteDto
    {
        public int idContacto { get; set; }
        //public int idPaciente { get; set; }
        public int celularPaciente { get; set; }
        public int? celularAcompananteP { get; set; }
        //public string estadoContacto { get; set; }
    }
}
