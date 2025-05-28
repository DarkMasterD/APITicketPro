using APITicketPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TecnicoController : Controller
    {
        private readonly DBTicketProContext _context;

        public TecnicoController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-tecnico/{idUsuarioInterno}")]
        public async Task<IActionResult> DashboardTecnico(int idUsuarioInterno)
        {
            var ticketsAsignados = await _context.tarea_ticket
                .Where(t => t.id_usuario_interno == idUsuarioInterno)
                .Select(t => t.id_ticket)
            .ToListAsync();

            var resumen = await _context.ticket
                .Where(t => ticketsAsignados.Contains(t.id_ticket))
                .GroupBy(t => t.estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
            .ToListAsync();

            var tickets = await _context.ticket
                .Where(t => ticketsAsignados.Contains(t.id_ticket))
                .Select(t => new
                {
                    t.id_ticket,
                    t.titulo,
                    t.estado
                })
                .ToListAsync();

            return Ok(new
            {
                Resumen = resumen,
                Tickets = tickets
            });
        }

    }
}
