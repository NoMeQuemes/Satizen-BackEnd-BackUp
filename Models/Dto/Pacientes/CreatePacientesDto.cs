namespace Satizen_Api.Models.Dto.Pacientes
{
    public class CreatePacientesDto
    {
        public int idUsuario {  get; set; }
        public int idInstitucion { get; set; }
        //public int idContacto { get; set; }
        public string? nombrePaciente { get; set; }
        public string? apellido { get; set; }
        public string? dni { get; set; }
        public string? direccionPaciente { get; set; }
        public int celularPaciente { get; set; }
        public int? celularAcompañante { get; set; }
        public int numeroHabitacionPaciente { get; set; }
        public string? observacionPaciente { get; set; }
    }
}
