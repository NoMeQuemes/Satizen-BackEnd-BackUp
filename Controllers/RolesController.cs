using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Acciones;
using Satizen_Api.Models.Dto.Roles;
using System.Net;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        protected ApiResponse _response;

        public RolesController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _response = new ApiResponse();
        }

        [Authorize(Policy = "Administrador")]
        [HttpGet]
        [Route("ListarRoles")]
        public async Task<ActionResult<ApiResponse>> GetRoles()
        {
            try
            {
                _response.Resultado = await _applicationDbContext.Roles
                    .Where(r => r.fechaEliminacion == null)
                    .ToListAsync();

                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Policy = "Administrador")]
        [HttpGet]
        [Route("ListarPorId/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolesDto>> GetRoles(int id)
        {


            var roles = await _applicationDbContext.Roles.FindAsync(id);

            if (roles == null)
            {
                return NotFound();
            }

            return Ok(roles);
        }

        [Authorize(Policy = "Administrador")]
        [HttpPost]
        [Route("CrearRol")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> CrearRol(CreateRolDto rolDto)
        {
            try
            {
                if (rolDto == null)
                {
                    return BadRequest(rolDto);
                }

                Roles modelo = new()
                {
                    nombre = rolDto.nombre,
                    descripcion = rolDto.descripcion
                };

                await _applicationDbContext.Roles.AddAsync(modelo);
                await _applicationDbContext.SaveChangesAsync();

                if (modelo.idRol != 0)
                {
                    _response.statusCode = HttpStatusCode.OK;
                    _response.IsExitoso = true;
                    _response.Resultado = modelo;
                }
                else
                {
                    _response.statusCode = HttpStatusCode.InternalServerError;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "Error al registrar el rol " };
                }

                return StatusCode((int)_response.statusCode, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return StatusCode((int)_response.statusCode, _response);
            }
        }

        [Authorize(Policy = "Administrador")]
        [HttpPut]
        [Route("ActualizarRol/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse>> ActualizarRol(int id, [FromBody] UpdateRolDto rolDto)
        {
            try
            {
                if (rolDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var rolExistente = await _applicationDbContext.Roles.FirstOrDefaultAsync(e => e.idRol == id);

                if (rolExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "El rol no existe" };
                    return _response;
                }

                rolExistente.nombre = rolDto.nombre;
                rolExistente.descripcion = rolDto.descripcion;

                _applicationDbContext.Roles.Update(rolExistente);
                await _applicationDbContext.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.NoContent;
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [Authorize(Policy = "Administrador")]
        [HttpPatch]
        [Route("DesactivarRol/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DesactivarRol(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var rol = await _applicationDbContext.Roles.FirstOrDefaultAsync(p => p.idRol == id);

            if (rol == null)
            {
                return NotFound();
            }

            rol.fechaEliminacion = DateTime.Now;

            _applicationDbContext.Roles.Update(rol);
            await _applicationDbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
