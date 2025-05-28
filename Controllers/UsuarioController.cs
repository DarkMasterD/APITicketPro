using Microsoft.AspNetCore.Mvc;
using APITicketPro.Models.Admin;
using Microsoft.EntityFrameworkCore;
using APITicketPro.Models;
using BCrypt.Net;
using System.Diagnostics;

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

        [HttpPost("crear-externo")]
        public async Task<IActionResult> CrearExterno([FromBody] UsuarioEditarViewModel model)
        {

            try
            {

                var nuevoUsuario = new usuario
                {
                    nombre_usuario = model.Usuario,
                    email = model.Email,
                    contrasenia = BCrypt.Net.BCrypt.HashPassword(model.Contrasena),
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

        [HttpGet("obtener-user/{id}")]
        public async Task<IActionResult> ObtenerUsuario(int id)
        {
            var usuario = await _context.usuario.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            if (usuario.tipo_usuario == 'E')
            {
                var externo = await _context.usuario_externo.FirstOrDefaultAsync(x => x.id_usuario == id);
                if (externo == null)
                    return NotFound("Usuario externo no encontrado.");

                return Ok(new UsuarioExternoViewModel
                {
                    IdUsuario = usuario.id_usuario,
                    Usuario = usuario.nombre_usuario,
                    Email = usuario.email,
                    Nombre = externo.nombre,
                    Apellido = externo.apellido,
                    Empresa = externo.empresa,
                    TipoUsuario = 'E'.ToString()
                });
            }
            else if (usuario.tipo_usuario == 'I')
            {
                var interno = await _context.usuario_interno.FirstOrDefaultAsync(x => x.id_usuario == id);
                if (interno == null)
                    return NotFound("Usuario interno no encontrado.");

                return Ok(new UsuarioInternoViewModel
                {
                    IdUsuario = usuario.id_usuario,
                    Usuario = usuario.nombre_usuario,
                    Email = usuario.email,
                    Nombre = interno.nombre,
                    Apellido = interno.apellido,
                    Direccion = interno.direccion,
                    Dui = interno.dui,
                    IdRol = interno.id_rol,
                    TipoUsuario = "I"
                });
            }

            return BadRequest("Tipo de usuario no reconocido.");
        }

        [HttpPut("actualizar")]
        public async Task<IActionResult> ActualizarUsuario([FromBody] UsuarioEditarViewModel model)
        {
            Debug.WriteLine($"=== Actualizando usuario {model.IdUsuario} ===");

            var usuario = await _context.usuario.FindAsync(model.IdUsuario);
            if (usuario == null)
            {
                Debug.WriteLine("⚠️ Usuario no encontrado");
                return NotFound("Usuario no encontrado.");
            }

            Debug.WriteLine($"Tipo de usuario: {usuario.tipo_usuario}");

            usuario.nombre_usuario = model.Usuario;
            usuario.email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Contrasena))
            {
                usuario.contrasenia = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);
            }

            if (usuario.tipo_usuario == 'E')
            {
                Debug.WriteLine("➡️ Es externo. Buscando en usuario_externo...");

                var externo = await _context.usuario_externo.FirstOrDefaultAsync(x => x.id_usuario == model.IdUsuario);
                if (externo != null)
                {
                    Debug.WriteLine("✅ Externo encontrado. Actualizando datos.");
                    externo.nombre = model.Nombre;
                    externo.apellido = model.Apellido;
                    externo.empresa = model.Empresa;
                }
                else
                {
                    Debug.WriteLine("❌ No se encontró el registro en usuario_externo.");
                }
            }
            else if (usuario.tipo_usuario == 'I')
            {
                Debug.WriteLine("➡️ Es interno.");

                var interno = await _context.usuario_interno.FirstOrDefaultAsync(x => x.id_usuario == model.IdUsuario);
                if (interno != null)
                {
                    interno.nombre = model.Nombre;
                    interno.apellido = model.Apellido;
                    interno.direccion = model.Direccion;
                    interno.dui = model.Dui;

                    if (model.IdRol.HasValue)
                        interno.id_rol = model.IdRol.Value;
                }
            }
            else
            {
                Debug.WriteLine("❌ Tipo de usuario inválido.");
                return BadRequest("Tipo de usuario no válido");
            }

            await _context.SaveChangesAsync();
            Debug.WriteLine("✔️ Cambios guardados.");

            return Ok("✅ Usuario actualizado correctamente.");

        }


        [HttpPost("crear-interno")]
        public async Task<IActionResult> CrearInterno([FromBody] UsuarioEditarViewModel model)
        {
            try
            {
                if (!model.IdRol.HasValue)
                    return BadRequest("Debe seleccionar un rol para el usuario interno.");

                var nuevoUsuario = new usuario
                {
                    nombre_usuario = model.Usuario,
                    email = model.Email,
                    contrasenia = BCrypt.Net.BCrypt.HashPassword(model.Contrasena),
                    tipo_usuario = 'I',
                    estado = true,
                    fecha_registro = DateTime.Now
                };

                _context.usuario.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                var interno = new usuario_interno
                {
                    id_usuario = nuevoUsuario.id_usuario,
                    nombre = model.Nombre,
                    apellido = model.Apellido,
                    direccion = model.Direccion,
                    dui = model.Dui,
                    id_rol = model.IdRol.Value
                };

                _context.usuario_interno.Add(interno);
                await _context.SaveChangesAsync();

                return Created("", new { idUsuario = nuevoUsuario.id_usuario });
            }
            catch (DbUpdateException dbEx)
            {
                var mensaje = dbEx.InnerException?.Message;

                if (mensaje != null && mensaje.Contains("UQ__usuario__D4D22D74"))
                    return Conflict("El nombre de usuario ya está en uso.");
                if (mensaje != null && mensaje.Contains("UQ__usuario__AB6E6164"))
                    return Conflict("El correo electrónico ya está registrado.");
                if (mensaje != null && mensaje.Contains("UQ__usuario_interno__DUI"))
                    return Conflict("El número de DUI ya está registrado.");

                return StatusCode(500, "Error al guardar: " + mensaje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error inesperado: " + ex.Message);
            }
        }

        [HttpDelete("eliminar/{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.usuario.FirstOrDefaultAsync(u => u.id_usuario == id);

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            if (usuario.tipo_usuario == 'I')
            {
                // Verifica si es un usuario interno administrador
                var interno = await _context.usuario_interno.FirstOrDefaultAsync(ui => ui.id_usuario == id);
                if (interno != null)
                {
                    var rol = await _context.rol.FirstOrDefaultAsync(r => r.id_rol == interno.id_rol);
                    if (rol != null && rol.nombre == "Administrador")
                        return BadRequest("❌ No se puede eliminar a un usuario administrador.");

                    // Primero se elimina usuario_interno
                    _context.usuario_interno.Remove(interno);
                }
            }
            else if (usuario.tipo_usuario == 'E')
            {
                var externo = await _context.usuario_externo.FirstOrDefaultAsync(ue => ue.id_usuario == id);
                if (externo != null)
                    _context.usuario_externo.Remove(externo);
            }

            // Eliminar contactos si los hay
            var contactos = await _context.contacto_usuario.Where(c => c.id_usuario == id).ToListAsync();
            _context.contacto_usuario.RemoveRange(contactos);

            // Finalmente eliminar el usuario
            _context.usuario.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok("✅ Usuario eliminado con éxito.");
        }




    }
}
