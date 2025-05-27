using Microsoft.AspNetCore.Mvc;
using APITicketPro.Models.Admin;
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

        [HttpGet("listar")]
        public async Task<IActionResult> ListarUsuarios([FromQuery] string busqueda = "", [FromQuery] int? rolId = null)
        {
            // Trae la lista básica de usuarios con sus datos base
            var usuariosBase = await _context.usuario
                .Select(u => new
                {
                    u.id_usuario,
                    u.email,
                    Interno = _context.usuario_interno.FirstOrDefault(i => i.id_usuario == u.id_usuario),
                    Externo = _context.usuario_externo.FirstOrDefault(e => e.id_usuario == u.id_usuario)
                })
                .ToListAsync(); 

            var usuarios = usuariosBase
                .Select(u => new UsuarioListadoDTO
                {
                    IdUsuario = u.id_usuario,
                    Correo = u.email,
                    Nombre = u.Interno != null ? $"{u.Interno.nombre} {u.Interno.apellido}" : $"{u.Externo?.nombre} {u.Externo?.apellido}",
                    Rol = u.Interno != null ? _context.rol.FirstOrDefault(r => r.id_rol == u.Interno.id_rol)?.nombre : "Externo",
                    Empresa = u.Externo?.empresa ?? "-"
                })
                .Where(u =>
                    (string.IsNullOrEmpty(busqueda) || u.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) &&
                    (!rolId.HasValue || _context.usuario_interno.Any(i => i.id_usuario == u.IdUsuario && i.id_rol == rolId.Value))
                )
                .ToList();
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

        [HttpPost("crear-externo")]
        public async Task<IActionResult> CrearExterno([FromBody] UsuarioExternoViewModel model)
        {

            try
            {

                var nuevoUsuario = new usuario
                {
                    nombre_usuario = model.Usuario,
                    email = model.Email,
                    contrasenia = model.Contrasena,
                    tipo_usuario = 'E',
                    estado = true,
                    fecha_registro = DateTime.Now
                };

                _context.usuario.Add(nuevoUsuario);
                await _context.SaveChangesAsync(); // Guarda y genera el id_usuario

                var externo = new usuario_externo
                {
                    id_usuario = nuevoUsuario.id_usuario,
                    nombre = model.Nombre,
                    apellido = model.Apellido,
                    empresa = model.Empresa
                };

                _context.usuario_externo.Add(externo);

                await _context.SaveChangesAsync();
                return Created("", new { idUsuario = nuevoUsuario.id_usuario });

                //return Ok(new { idUsuario = nuevoUsuario.id_usuario });
            }
            catch (DbUpdateException dbEx)
            {
                var mensaje = dbEx.InnerException?.Message;

                if (mensaje != null && mensaje.Contains("UQ__usuario__D4D22D74A93D9195")) 
                {
                    return Conflict("El nombre de usuario ya está en uso.");
                }
                if (mensaje != null && mensaje.Contains("UQ__usuario__AB6E616445F6A6C4"))
                {
                    return Conflict("El correo electrónico ya está registrado.");
                }

                return StatusCode(500, "Error al guardar en la base de datos: " + mensaje);
            }

        }

        [HttpGet("externo/{id}")]
        public async Task<IActionResult> ObtenerUsuarioExterno(int id)
        {
            var usuario = await _context.usuario
                .Where(u => u.id_usuario == id && u.tipo_usuario == 'E')
                .Join(_context.usuario_externo,
                      u => u.id_usuario,
                      ue => ue.id_usuario,
                      (u, ue) => new UsuarioExternoViewModel
                      {
                          IdUsuario = u.id_usuario, 
                          Usuario = u.nombre_usuario,
                          Email = u.email,
                          Contrasena = u.contrasenia,
                          TipoUsuario = u.tipo_usuario.ToString(),
                          Nombre = ue.nombre,
                          Apellido = ue.apellido,
                          Empresa = ue.empresa
                      })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            return Ok(usuario);
        }


        [HttpPut("actualizar-externo")]
        public async Task<IActionResult> ActualizarUsuarioExterno([FromBody] UsuarioExternoViewModel model)
        {
            if (model.IdUsuario == null) return BadRequest("Id de usuario requerido.");

            var usuario = await _context.usuario.FindAsync(model.IdUsuario);
            if (usuario == null) return NotFound();

            usuario.nombre_usuario = model.Usuario;
            usuario.email = model.Email;

            // Solo actualizar si se proporcionó una nueva contraseña
            if (!string.IsNullOrWhiteSpace(model.Contrasena))
            {
                usuario.contrasenia = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);
            }

            var externo = await _context.usuario_externo
                .FirstOrDefaultAsync(x => x.id_usuario == model.IdUsuario);

            if (externo != null)
            {
                externo.nombre = model.Nombre;
                externo.apellido = model.Apellido;
                externo.empresa = model.Empresa;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }




    }
}
