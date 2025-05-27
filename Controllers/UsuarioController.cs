using APITicketPro.Models;
using APITicketPro.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly DBTicketProContext _context;

        private readonly HttpClient _http;
        public UsuarioController(DBTicketProContext context)
        {
            _context = context;
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
            usuario.contrasenia = model.Contrasena;

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
