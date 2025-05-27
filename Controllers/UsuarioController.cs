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
                           select new TecnicoPerfilViewModel
                           {
                               Nombres = ui.nombre,
                               Apellidos = ui.apellido,
                               Direccion = ui.direccion,
                               Usuario = u.nombre_usuario,
                               FechaRegistro = u.fecha_registro,
                               Contactos = _context.contacto_usuario
                                   .Where(c => c.id_usuario == id_usuario)
                                   .Select(c => new ContactoViewModel
                                   {
                                       Id = c.id_contacto_usuario,
                                       Email = c.email,
                                       Telefono = c.telefono
                                   }).ToList()
                           }).FirstOrDefault();

            return Ok(tecnico);
        }

        [HttpGet("contactos/{id_usuario}")]
        public async Task<IActionResult> ObtenerContactosPorUsuario(int id_usuario)
        {
            var contactos = await _context.contacto_usuario
                .Where(c => c.id_usuario == id_usuario)
                .Select(c => new
                {
                    id_contacto_usuario = c.id_contacto_usuario,
                    email = c.email,
                    telefono = c.telefono
                })
                .ToListAsync();

            return Ok(contactos);
        }


        [HttpPost("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] TecnicoPerfilViewModel dto)
        {
            var usuario = await _context.usuario.FindAsync(dto.IdUsuario);
            var interno = await _context.usuario_interno.FirstOrDefaultAsync(ui => ui.id_usuario == dto.IdUsuario);

            if (usuario == null || interno == null)
                return NotFound("Técnico no encontrado");

            usuario.nombre_usuario = dto.Usuario;
            interno.nombre = dto.Nombres;
            interno.apellido = dto.Apellidos;
            interno.direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return Ok("Perfil actualizado correctamente");
        }


        [HttpPost("agregar-contactos")]
        public async Task<IActionResult> AgregarContactos([FromBody] List<NuevoContactoDTO> contactos)
        {
            if (contactos == null || contactos.Count == 0)
                return BadRequest("No se proporcionaron contactos");

            foreach (var c in contactos)
            {
                if (string.IsNullOrWhiteSpace(c.Email) && string.IsNullOrWhiteSpace(c.Telefono))
                    continue;

                var nuevo = new contacto_usuario
                {
                    id_usuario = c.IdUsuario,
                    email = c.Email,
                    telefono = c.Telefono
                };

                _context.contacto_usuario.Add(nuevo);
            }

            await _context.SaveChangesAsync();
            return Ok("Contactos agregados correctamente");
        }


        [HttpPut("actualizar-contacto")]
        public async Task<IActionResult> ActualizarContacto([FromBody] ContactoViewModel dto)
        {
            var contacto = await _context.contacto_usuario.FindAsync(dto.Id);
            if (contacto == null)
                return NotFound("Contacto no encontrado");

            contacto.email = dto.Email;
            contacto.telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            return Ok("Contacto actualizado correctamente");
        }

    }
}
