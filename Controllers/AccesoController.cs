﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satizen_Api.Custom;
using Satizen_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Satizen_Api.Data;
using Satizen_Api.Models.Dto.Usuarios;
using System.Net;
using Azure;
using Prueba_Tecnica_Api.Models.Dto.Usuarios;

//Using para enviar correos
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous] //Esta sentencia define que no es necesario estar autorizado para usar este controlador
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly Utilidades _utilidades;
        protected ApiResponse _response;
        private readonly IWebHostEnvironment _enviroment;
        private readonly IConfiguration _configuration;

        public AccesoController(ApplicationDbContext applicationDbContext, Utilidades utilidades, IWebHostEnvironment enviroment, IConfiguration configuration)
        {
            _applicationDbContext = applicationDbContext;
            _utilidades = utilidades;
            _response = new();
            _enviroment = enviroment;
            _configuration = configuration;
        }

        //Registrar a un usuario
        [HttpPost]
        [Route("RegistrarUsuario")]
        public async Task<ActionResult<ApiResponse>> Registrarse([FromForm]UsuarioCreateDto objeto)
        {
            try
            {
                //Acá se valida que no exista el nombre de usuario
                if (await _applicationDbContext.Usuarios.AnyAsync(u => u.nombreUsuario == objeto.nombreUsuario))
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "El nombre de usuarios ya existe" };
                    return StatusCode((int)_response.statusCode, _response);
                }

                var modeloUsuario = new Usuario
                {
                    nombreUsuario = objeto.nombreUsuario,
                    correo = objeto.correo,
                    password = _utilidades.encriptarSHA256(objeto.password),
                    celularUsuario = objeto.celularUsuario,
                    idRoles = objeto.idRoles,
                    fechaCreacion = DateTime.Now,
                };

                try
                {
                    if (objeto.imagenPefil != null)
                    {
                        var imagenUrl = await SaveImage(objeto.imagenPefil);
                        modeloUsuario.imagenPerfilUrl = imagenUrl;
                    }
                }
                catch(ArgumentException ex)
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "El archivo debe ser una imagen." };
                    return StatusCode((int)_response.statusCode, _response);
                }

                await _applicationDbContext.Usuarios.AddAsync(modeloUsuario);
                await _applicationDbContext.SaveChangesAsync();
                if (modeloUsuario.idUsuario != 0)
                {
                    _response.statusCode = HttpStatusCode.OK;
                    _response.IsExitoso = true;
                    _response.Resultado = modeloUsuario;
                }
                else
                {
                    _response.statusCode = HttpStatusCode.InternalServerError;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "Error al registrar el usuario." };
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

        // EndPoint que loggea al usuario
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDto objeto)
        {
            // Filtrar usuarios por contraseña y estado (eliminación)
            var usuariosFiltrados = await _applicationDbContext.Usuarios
                                        .Where(u =>
                                            u.password == _utilidades.encriptarSHA256(objeto.password) &&
                                            u.fechaEliminacion == null)
                                        .ToListAsync();

            // Realizar comparación estricta por nombre de usuario
            var usuarioEncontrado = usuariosFiltrados.FirstOrDefault(u =>
                                        u.nombreUsuario.Equals(objeto.nombreUsuario, StringComparison.Ordinal));

            if (usuarioEncontrado == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, token = "", refreshToken = "" });
            }
            else
            {
                // Generar JWT
                var token = _utilidades.generarJWT(usuarioEncontrado);

                // Generar Refresh Token
                var refreshToken = _utilidades.generarRefreshJWT();

                // Guardar el refresh token en la base de datos
                var response = await _utilidades.GuardarRefreshToken(usuarioEncontrado.idUsuario, token, refreshToken);

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = response.Token, refreshToken = response.RefreshToken });
            }
        }


        //Generar refresh token
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenRequest)
        {
            var storedRefreshToken = await _applicationDbContext.RefreshTokens
                                            .FirstOrDefaultAsync(r => r.refreshToken == refreshTokenRequest.RefreshToken);

            if (storedRefreshToken == null || storedRefreshToken.fechaExpiracion <= DateTime.UtcNow || !storedRefreshToken.esActivo)
            {
                return Unauthorized(new { isSuccess = false, message = "Invalid or expired refresh token" });
            }

            var usuario = await _applicationDbContext.Usuarios
                                .FirstOrDefaultAsync(u => u.idUsuario == storedRefreshToken.idUsuario);

            if (usuario == null)
            {
                return Unauthorized(new { isSuccess = false, message = "Invalid user" });
            }

            // Generar nuevo JWT
            var newToken = _utilidades.generarJWT(usuario);

            // Generar nuevo Refresh Token
            var newRefreshToken = _utilidades.generarRefreshJWT();

            // Desactivar el refresh token anterior
            storedRefreshToken.esActivo = false;
            _applicationDbContext.RefreshTokens.Update(storedRefreshToken);

            // Guardar el nuevo refresh token
            var response = await _utilidades.GuardarRefreshToken(usuario.idUsuario, newToken, newRefreshToken);

            return Ok(new { isSuccess = true, token = response.Token, refreshToken = response.RefreshToken });
        }

        //Recuperar contraseña
        [HttpPatch]
        [Route("RecuperarContraseña")]
        public async Task<ActionResult<ApiResponse>> Restaurar(RecuperarPasswordDto recuperarDto)
        {
            try
            {
                var usuario = await _applicationDbContext.Usuarios.FirstOrDefaultAsync(u => u.correo == recuperarDto.correo && u.fechaEliminacion == null);
                if (usuario == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "El correo no existe" };

                    return BadRequest(_response);
                }

                //var token = GetSha256(Guid.NewGuid().ToString());

                usuario.recoveryToken = GetSha256(Guid.NewGuid().ToString());


                _applicationDbContext.Usuarios.Update(usuario);
                await _applicationDbContext.SaveChangesAsync();
                //Enviar Email
                await SendEmail(usuario.correo, usuario.recoveryToken, _configuration);

                _response.statusCode = HttpStatusCode.OK;
                _response.IsExitoso = true;
                _response.Resultado = usuario;

                return StatusCode((int)_response.statusCode, _response);

            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return StatusCode((int)_response.statusCode, _response);
            }
        }

        //Cambiar contraseña
        [HttpPatch]
        [Route("CambiarContraseña")]
        public async Task<ActionResult<ApiResponse>> Cambiar(CambiarPasswordDto cambiarDto)
        {
            try
            {
                var usuario = await _applicationDbContext.Usuarios.FirstOrDefaultAsync(u => u.recoveryToken == cambiarDto.recoveryToken);
                if(usuario == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode =  HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "El token no es correcto o ha vencido" };

                    return BadRequest(_response);
                }

                usuario.password = _utilidades.encriptarSHA256(cambiarDto.password);
                usuario.recoveryToken = null;

                _applicationDbContext.Usuarios.Update(usuario);
                await _applicationDbContext.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.OK;
                _response.IsExitoso = true;
                _response.Resultado = usuario;

                return StatusCode((int)_response.statusCode, _response);

            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return StatusCode((int)_response.statusCode, _response);
            }
        }

        //Generar recovery token
        private string GetSha256(string cadena)
        {
            SHA256 sha256 = SHA256.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(cadena));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        //Generar correo electronico
        static async Task SendEmail(string emailDestino, string token, IConfiguration configuration)
        {
            //var apiKey = Environment.GetEnvironmentVariable("SatizenCorreoClave");
            //string apiKey = "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";
            string apiKey = configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            string url = "http://localhost:8081/cambiarPassword/?token=" + token;
            //string url = "https://animated-panda-c5f730.netlify.app/cambiarPassword/?token=" + token;

            var from = new EmailAddress("satizencorp@gmail.com", "Satizen");
            var subject = "Recuperar Contraseña - Satizen"; //Este es el asunto del correo
            var to = new EmailAddress(emailDestino);
            var plainTextContent = "Recuperar contraseña - Satizen";
            var htmlContent = "<p>Correo para recuperar la contraseña de Satizen</p> </br> <a href='" + url + "'>Haz click aquí</a>"; //Este es el cuerpo del correo
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Correo enviado correctamente");
            }
            else
            {
                Console.WriteLine($"Error al enviar el correo: {response.StatusCode}");
            }
        }

        //Guardar la imagen de perfil
        private async Task<string> SaveImage(IFormFile image)
        {
            try
            {
                if (!image.ContentType.ToLower().StartsWith("image/"))
                {
                    throw new ArgumentException("El archivo debe ser una imagen.");
                }

                var uploadsFolder = Path.Combine(_enviroment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                return Path.Combine("uploads", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la imagen: {ex.Message}");
                throw; // Lanza nuevamente la excepción para que puedas manejarla en el controlador
            }
        }
    }
}
