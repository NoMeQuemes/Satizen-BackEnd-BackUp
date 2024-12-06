using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Mensaje;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensajesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MensajesController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/Mensajes
       [HttpGet]
       public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajes()
        {
            return await _context.Mensajes.OrderBy(m => m.Timestamp).ToListAsync();
        }


        // GET: api/Mensajes/EntreUsuarios/{idAutor}/{idReceptor}
        [HttpGet("EntreUsuarios/{idAutor}/{idReceptor}")]
        public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajesEntreUsuarios(int idAutor, int idReceptor)
        {
            var mensajes = await _context.Mensajes
                .Where(m => (m.idAutor == idAutor && m.idReceptor == idReceptor) || (m.idAutor == idReceptor && m.idReceptor == idAutor))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            if (mensajes == null || !mensajes.Any())
            {
                return Ok(); 
            }

            return mensajes;
        }

        // POST: api/Mensajes
        [HttpPost]
        public async Task<ActionResult<Mensaje>> PostMensaje(CreateMensajeDto CreateMensajeDto)
        {
            var autor = await _context.Usuarios.FindAsync(CreateMensajeDto.idAutor);
            var receptor = await _context.Usuarios.FindAsync(CreateMensajeDto.idReceptor);

            if (autor == null)
            {
                return BadRequest("El autor seleccionado no existe.");
            }

            if (receptor == null)
            {
                return BadRequest("El receptor seleccionado no existe.");
            }

            var nuevoMensaje = new Mensaje
            {
                idAutor = CreateMensajeDto.idAutor,
                idReceptor = CreateMensajeDto.idReceptor,
                contenidoMensaje = CreateMensajeDto.contenidoMensaje,
                Timestamp = DateTime.UtcNow,
                Enviado = true, 
                Visto = false   
            };

            _context.Mensajes.Add(nuevoMensaje);
            await _context.SaveChangesAsync();

            string groupName = $"{Math.Min(nuevoMensaje.idAutor, nuevoMensaje.idReceptor)}-{Math.Max(nuevoMensaje.idAutor, nuevoMensaje.idReceptor)}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", nuevoMensaje.idAutor, nuevoMensaje.idReceptor, nuevoMensaje.contenidoMensaje, nuevoMensaje.Timestamp , nuevoMensaje.Visto);

            return CreatedAtAction(nameof(GetMensajes), new { id = nuevoMensaje.Id }, nuevoMensaje);
        }




        // PUT: api/Mensajes/MarcarComoVisto/{idAutor}/{idReceptor}

        [HttpPut("MarcarComoVisto/{idAutor}/{idReceptor}")]
        public async Task<ActionResult<IEnumerable<Mensaje>>> MarcarComoVisto(int idAutor, int idReceptor)
        {
            var mensajes = await _context.Mensajes
                .Where(m => (m.idAutor == idAutor && m.idReceptor == idReceptor) || (m.idAutor == idReceptor && m.idReceptor == idAutor))
                .ToListAsync();

            if (mensajes == null || !mensajes.Any())
            {
                return Ok();
            }

            if (mensajes.Count == 0)
            {
                return NotFound("No se encontraron mensajes entre los usuarios especificados.");
            }

            foreach (var mensaje in mensajes)
            {
                mensaje.Visto = true;
            }

            await _context.SaveChangesAsync();
            string groupName = $"{Math.Min(idAutor, idReceptor)}-{Math.Max(idAutor, idReceptor)}";


            return Ok(mensajes);
        }

        [HttpPut("MarcarComoEnviado/{idAutor}")]
        public async Task<ActionResult<IEnumerable<Mensaje>>> MarcarComoEnviado(int idAutor)
        {
            var mensajes = await _context.Mensajes
                .Where(m => (m.idAutor == idAutor))
                .ToListAsync();

            if (mensajes == null || !mensajes.Any())
            {
                return Ok();
            }

            if (mensajes.Count == 0)
            {
                return NotFound("No se encontraron mensajes entre los usuarios especificados.");
            }

            foreach (var mensaje in mensajes)
            {
                mensaje.Enviado = true;
            }

            await _context.SaveChangesAsync(); 



            return Ok(mensajes);
        }


    }
}
