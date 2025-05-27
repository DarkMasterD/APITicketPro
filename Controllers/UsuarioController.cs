using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITicketPro.Models;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller
    {
        private readonly DBTicketProContext _context;

        public UsuarioController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
        {
            var usuarios = await _context.usuario
                .Include(u => u.contactos)
                .Select(u => new
                {
                    id_usuario = u.id_usuario,
                    nombre_usuario = u.nombre_usuario,
                    email = u.email,
                    tipo_usuario = u.tipo_usuario,
                    telefono = u.contactos
                        .OrderByDescending(c => c.id_contacto_usuario)
                        .FirstOrDefault().telefono
                })
                .ToListAsync();


            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUsuarioPorId(int id)
        {
            var usuario = await _context.usuario
                .Include(u => u.contactos)
                .Where(u => u.id_usuario == id)
                .Select(u => new
                {
                    id_usuario = u.id_usuario,
                    nombre_usuario = u.nombre_usuario,
                    email = u.email,
                    tipo_usuario = u.tipo_usuario,
                    telefono = u.contactos
                        .OrderByDescending(c => c.id_contacto_usuario)
                        .FirstOrDefault().telefono
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario);
        }

        [HttpPost("cambiar-contrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDTO dto)
        {
            var usuario = await _context.usuario.FindAsync(dto.IdUsuario);
            if (usuario == null) return NotFound("Usuario no encontrado");

            if (!BCrypt.Net.BCrypt.Verify(dto.Actual, usuario.contrasenia))
                return BadRequest("La contraseña actual es incorrecta");

            usuario.contrasenia = BCrypt.Net.BCrypt.HashPassword(dto.Nueva);
            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada correctamente");
        }



        [HttpGet("perfil-tecnico/{id_usuario}")]
        public IActionResult PerfilTecnico(int id_usuario)
        {
            var tecnico = (from u in _context.usuario
                           join ui in _context.usuario_interno on u.id_usuario equals ui.id_usuario
                           where u.id_usuario == id_usuario
                           select new
                           {
                               Nombre = ui.nombre + " " + ui.apellido,
                               Direccion = ui.direccion,
                               Usuario = u.nombre_usuario,
                               FechaRegistro = u.fecha_registro
                           }).FirstOrDefault();

            if (tecnico == null)
                return NotFound("Técnico no encontrado");

            return Ok(tecnico);
        }

        [HttpGet("contacto/{id}")]
        public async Task<IActionResult> ObtenerContactoPorId(int id)
        {
            var contacto = await _context.contacto_usuario
                .Where(c => c.id_usuario == id)
                .OrderByDescending(c => c.id_contacto_usuario)
                .FirstOrDefaultAsync();

            if (contacto == null)
                return NotFound();

            return Ok(new
            {
                email = contacto.email,
                telefono = contacto.telefono
            });
        }

    }
}
