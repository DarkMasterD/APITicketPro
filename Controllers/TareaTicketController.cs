using APITicketPro.Models;
using APITicketPro.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace APITicketPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareaTicketController : Controller
    {
        private readonly DBTicketProContext _context;

        private readonly HttpClient _http;
        public TareaTicketController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> CrearTarea([FromBody] CrearTareaDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var nuevaTarea = new tarea_ticket
                {
                    id_ticket = dto.IdTicket,
                    id_usuario_interno = dto.IdTecnico,
                    nombre = dto.Titulo,
                    descripcion = dto.Descripcion,
                    estado = "Asignada",
                    fecha_inicio = DateTime.Now
                };
                var existeTicket = await _context.ticket.AnyAsync(t => t.id_ticket == dto.IdTicket);
                var existeTecnico = await _context.usuario_interno.AnyAsync(u => u.id_usuario_interno == dto.IdTecnico);

                if (!existeTicket)
                    return BadRequest("El ticket no existe.");

                if (!existeTecnico)
                    return BadRequest("El técnico no existe.");


                _context.tarea_ticket.Add(nuevaTarea);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Tarea creada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message} - {ex.InnerException?.Message}");
            }
        }


        [HttpGet("usuarios-internos")]
        public IActionResult ObtenerTecnicos()
        {
            var tecnicos = _context.usuario_interno
                .Select(u => new TecnicoDTO
                {
                    Id = u.id_usuario_interno,
                    Nombre = u.nombre + " " + u.apellido
                })
                .ToList();

            return Ok(tecnicos);
        }



    }

}
