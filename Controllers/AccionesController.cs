using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Acciones;
using Satizen_Api.Models.Dto.Pacientes;
using Satizen_Api.Models.Dto.Usuarios;
using System.Net;

namespace Satizen_Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccionesController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        protected ApiResponse _response;

        public AccionesController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _response = new ApiResponse();
        }

        //--------------- EndPoint que trae la lista completa de acciones -------------------
        //[Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarAcciones")]
        public async Task<ActionResult<ApiResponse>> GetAcciones()
        {
            try
            {
                _response.Resultado = await _applicationDbContext.Acciones
                    .Include(r => r.Roles)
                    .Include(p => p.Permiso)
                    .Select( r => new
                    {
                        r.idAcciones,
                        Roles = r.Roles.nombre,
                        Permisos = r.Permiso.tipo
                    })
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

        //[Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpGet]
        [Route("ListarPorId/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccionesDto>> GetAccion(int id)
        {


            var accion = await _applicationDbContext.Acciones.FindAsync(id);

            if (accion == null)
            {
                return NotFound();
            }

            return Ok(accion);
        }

        //[Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpPost]
        [Route("CrearAccion")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> CrearAccion(AccionCreateDto accionDto)
        {
            try
            {
                if (accionDto == null)
                {
                    return BadRequest(accionDto);
                }

                Accion modelo = new()
                {
                    idRoles = accionDto.idRoles,
                    idPermiso = accionDto.idPermiso
                };

                await _applicationDbContext.Acciones.AddAsync(modelo);
                await _applicationDbContext.SaveChangesAsync();

                if (modelo.idAcciones != 0)
                {
                    _response.statusCode = HttpStatusCode.OK;
                    _response.IsExitoso = true;
                    _response.Resultado = modelo;
                }
                else
                {
                    _response.statusCode = HttpStatusCode.InternalServerError;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "Error al registrar la acción " };
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


        //[Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpPut]
        [Route("ActualizarAccion/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse>> ActualizarAccion(int id, [FromBody] AccionUpdateDto accionDto)
        {
            try
            {
                if (accionDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var accionExistente = await _applicationDbContext.Acciones.FirstOrDefaultAsync(e => e.idAcciones == id);

                if (accionExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "La acción no existe" };
                    return _response;
                }

                accionExistente.idRoles = accionDto.idRoles;
                accionExistente.idPermiso = accionDto.idPermiso;

                _applicationDbContext.Acciones.Update(accionExistente);
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

        [HttpDelete]
        [Route("EliminarPaciente/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> EliminarAccion(int id)
        {
            try
            {
                // Buscar el paciente en la base de datos por su ID
                var accion = await _applicationDbContext.Acciones.FindAsync(id);

                if (accion == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "Accion no encontrada" };
                    return NotFound(_response);
                }

                _applicationDbContext.Acciones.Remove(accion);
                await _applicationDbContext.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.OK;
                _response.IsExitoso = true;
                _response.Resultado = $"Accion {id} eliminada correctamente";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };

                return StatusCode((int)_response.statusCode, _response);
            }
        }

    }
}
