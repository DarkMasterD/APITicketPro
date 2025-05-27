using APITicketPro.Models;
using APITicketPro.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Controllers
{
    [ApiController]
    [Route("api/contactos")]
    public class ContactosController : Controller
    {
        private readonly DBTicketProContext _context;

        public ContactosController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet("usuario/{id}")]
        public async Task<IActionResult> ObtenerContactosPorUsuario(int id)
        {
            try
            {
                var contactos = await _context.contacto_usuario
                    .Where(c => c.id_usuario == id)
                    .ToListAsync();

                return Ok(contactos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR: " + ex.ToString());
                return StatusCode(500, "Error interno al obtener contactos");
            }
        }



        [HttpPost("crear")]
        public async Task<IActionResult> CrearContacto([FromBody] CrearContactoDTO model)
        {
            Console.WriteLine($"rear contacto: usuario={model.id_usuario}, email={model.email}, tel={model.telefono}");

            if (string.IsNullOrWhiteSpace(model.email) && string.IsNullOrWhiteSpace(model.telefono))
            {
                return BadRequest("Debe ingresar un correo o un teléfono.");
            }

            var contacto = new contacto_usuario
            {
                id_usuario = model.id_usuario,
                email = model.email,
                telefono = model.telefono
            };

            try
            {
                _context.contacto_usuario.Add(contacto);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al guardar contacto: " + ex.Message);
                return StatusCode(500, "Error interno");
            }
        }


    }
}
