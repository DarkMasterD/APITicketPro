using APITicketPro.Models;
using APITicketPro.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {
        private readonly DBTicketProContext _context;

        public TicketController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tickets = await _context.ticket.ToListAsync();
            return Ok(tickets);
        }

        // Método para obtener el contador de tickets del dashboard de administrador
        [HttpGet("resumen-dashboard")]
        public IActionResult ObtenerResumeDashboard()
        {
            var resumen = new DashboardResumenDTO
            {
                TicketsNoAsignados = _context.ticket.Count(t => t.estado == "No asignado"),
                TicketsEnProgreso = _context.ticket.Count(t => t.estado == "En Progreso"),
                TicketsCriticos = _context.ticket.Count(t => t.prioridad == "Crítico"),
                TicketsResueltos = _context.ticket.Count(t => t.estado == "Resuelto")
            };

            return Ok(resumen);
        }

        [HttpGet("resumen-tickets")]
        public IActionResult ObtenerTicketsDashboard()
        {
            var tickets = _context.ticket
                .Include(t => t.usuario)
                    .ThenInclude(u => u.usuario_externo)
                .Include(t => t.tareas)
                    .ThenInclude(tt => tt.usuario_interno)
                .Select(t => new TicketResumenDTO
                {
                    Titulo = t.titulo,
                    Cliente = t.usuario.usuario_externo.nombre + " " + t.usuario.usuario_externo.apellido,
                    Tecnico = t.tareas.FirstOrDefault().usuario_interno.nombre ?? "No asignado",
                    Estado = t.estado,
                    Prioridad = t.prioridad,
                    Fecha = t.fecha_inicio
                })
                .ToList();

            return Ok(tickets);
        }

        // Crear Tickets


    }
}
