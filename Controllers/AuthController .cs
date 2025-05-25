using Microsoft.AspNetCore.Mvc;
using APITicketPro.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;

namespace APITicketPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DBTicketProContext _context;

        public AuthController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] APITicketPro.Models.LoginRequest request)
        {
            var usuario = _context.usuario.FirstOrDefault(u =>
                u.nombre_usuario == request.Usuario && u.estado);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado o inactivo.");

            if (!BCrypt.Net.BCrypt.Verify(request.Contrasenia, usuario.contrasenia))
                return Unauthorized("Contraseña incorrecta.");

            return Ok(new
            {
                id = usuario.id_usuario,
                usuario = usuario.nombre_usuario,
                tipo = usuario.tipo_usuario
            });
        }

        [HttpGet("rol/{id_usuario}")]
        public IActionResult ObtenerRolUsuario(int id_usuario)
        {
            var interno = _context.usuario_interno
                .Include(u => u.rol)
                .FirstOrDefault(u => u.id_usuario == id_usuario);

            if (interno != null)
                return Ok(interno.rol.nombre);

            return Ok("Cliente");
        }
    }
}
