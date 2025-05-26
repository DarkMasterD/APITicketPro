using APITicketPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Runtime;
using System.Text.Json;
using System.Text;

namespace APITicketPro.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : Controller
    {
        private readonly DBTicketProContext _context;

        public ClienteController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet("nombre/{id_usuario}")]
        public async Task<IActionResult> ObtenerNombre(int id_usuario)
        {
            var cliente = await _context.usuario_externo
                .FirstOrDefaultAsync(u => u.id_usuario == id_usuario);

            if (cliente == null)
                return NotFound();

            return Ok(new { nombre = cliente.nombre });
        }

        [HttpGet("resumen/{id_usuario}")]
        public async Task<IActionResult> Resumen(int id_usuario)
        {
            var resumen = new
            {
                no_asignado = await _context.ticket.CountAsync(t => t.id_usuario == id_usuario && t.estado == "No asignado"),
                en_progreso = await _context.ticket.CountAsync(t => t.id_usuario == id_usuario && t.estado == "En Progreso"),
                resuelto = await _context.ticket.CountAsync(t => t.id_usuario == id_usuario && t.estado == "Resuelto")
            };

            return Ok(resumen);
        }

        [HttpGet("ultimos/{id_usuario}")]
        public async Task<IActionResult> UltimosTickets(int id_usuario)
        {
            var tickets = await _context.ticket
                .Where(t => t.id_usuario == id_usuario)
                .OrderByDescending(t => t.fecha_inicio)
                .Take(10)
                .Select(t => new
                {
                    t.titulo,
                    t.estado,
                    t.codigo,
                    t.id_ticket
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("info/{id_usuario}")]
        public async Task<IActionResult> ObtenerInformacionCliente(int id_usuario)
        {
            var usuario = await _context.usuario
                .Where(u => u.id_usuario == id_usuario)
                .Select(u => new
                {
                    u.nombre_usuario,
                    u.email,
                    fecha_registro = u.fecha_registro.ToString("yyyy-MM-dd")
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            var externo = await _context.usuario_externo
                .Where(e => e.id_usuario == id_usuario)
                .Select(e => new
                {
                    e.nombre,
                    e.apellido,
                    e.empresa
                })
                .FirstOrDefaultAsync();

            if (externo == null)
                return NotFound("Usuario externo no encontrado.");

            var resultado = new
            {
                usuario.nombre_usuario,
                usuario.email,
                usuario.fecha_registro,
                externo.nombre,
                externo.apellido,
                externo.empresa
            };

            return Ok(resultado);
        }

        [HttpPut("info/{id_usuario}")]
        public async Task<IActionResult> ActualizarInformacionCliente(int id_usuario, [FromBody] JsonElement data)
        {
            var usuario = await _context.usuario.FindAsync(id_usuario);
            var externo = await _context.usuario_externo.FirstOrDefaultAsync(e => e.id_usuario == id_usuario);

            if (usuario == null || externo == null)
                return NotFound();

            usuario.nombre_usuario = data.GetProperty("NombreUsuario").GetString();
            externo.nombre = data.GetProperty("Nombre").GetString();
            externo.apellido = data.GetProperty("Apellido").GetString();
            externo.empresa = data.GetProperty("Empresa").GetString();

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("contacto/{id_usuario}")]
        public async Task<IActionResult> ObtenerContactos(int id_usuario)
        {
            var contactos = await _context.contacto_usuario
                .Where(c => c.id_usuario == id_usuario)
                .Select(c => new
                {
                    c.id_contacto_usuario,
                    c.email,
                    c.telefono
                })
                .ToListAsync();

            return Ok(contactos);
        }
        [HttpPost("contacto")]
        public async Task<IActionResult> AgregarContacto([FromBody] contacto_usuario contacto)
        {
            // Validar: al menos uno debe tener valor
            bool correoVacio = string.IsNullOrWhiteSpace(contacto.email);
            bool telefonoVacio = string.IsNullOrWhiteSpace(contacto.telefono);

            if (correoVacio && telefonoVacio)
                return BadRequest("Debe ingresar al menos un correo o un teléfono.");

            if (contacto.id_usuario == 0)
                return BadRequest("Falta id_usuario.");

            _context.contacto_usuario.Add(contacto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("cambiar-contrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] JsonElement datos)
        {
            int idUsuario = datos.GetProperty("id_usuario").GetInt32();
            string actual = datos.GetProperty("actual").GetString();
            string nueva = datos.GetProperty("nueva").GetString();
            string confirmacion = datos.GetProperty("confirmacion").GetString();

            if (nueva != confirmacion)
                return BadRequest("La nueva contraseña y su confirmación no coinciden.");

            var usuario = await _context.usuario.FindAsync(idUsuario);
            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(actual, usuario.contrasenia))
                return BadRequest("La contraseña actual no es válida.");

            usuario.contrasenia = BCrypt.Net.BCrypt.HashPassword(nueva);
            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada.");
        }

    }
}
