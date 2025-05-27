using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITicketPro.Models;
using APITicketPro.Models.Admin;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardFiltroController : ControllerBase
    {
        private readonly DBTicketProContext _context;

        public DashboardFiltroController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet("tickets-filtrados")]
        public IActionResult ObtenerTicketsFiltrados([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] string? categoria, [FromQuery] string? prioridad, [FromQuery] string? estado)
        {
            var query = _context.ticket
                .Include(t => t.usuario)
                    .ThenInclude(u => u.usuario_externo)
                .Include(t => t.tareas)
                    .ThenInclude(tt => tt.usuario_interno)
                .Include(t => t.categoria_ticket)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(t => t.fecha_inicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
            {
                var finDelDia = fechaFin.Value.Date.AddDays(1).AddTicks(-1); // 23:59:59.999
                query = query.Where(t => t.fecha_inicio <= finDelDia);
            }


            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(t => t.categoria_ticket.nombre.Contains(categoria));

            if (!string.IsNullOrEmpty(prioridad))
                query = query.Where(t => t.prioridad == prioridad);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(t => t.estado == estado);

            var resultado = query.Select(t => new TicketResumenDTO
            {
                IdTicket = t.id_ticket,
                Titulo = t.titulo,
                Cliente = t.usuario.usuario_externo.nombre + " " + t.usuario.usuario_externo.apellido,
                Tecnico = t.tareas.FirstOrDefault().usuario_interno.nombre ?? "No asignado",
                Estado = t.estado,
                Prioridad = t.prioridad,
                Fecha = t.fecha_inicio
            }).ToList();

            return Ok(resultado);
        }
        [HttpGet("resumen-dashboard-filtrado")]
        public IActionResult ObtenerResumenFiltrado([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] string? categoria, [FromQuery] string? prioridad, [FromQuery] string? estado)
        {
            var query = _context.ticket
                .Include(t => t.categoria_ticket)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(t => t.fecha_inicio >= fechaInicio.Value);
            if (fechaFin.HasValue)
                query = query.Where(t => t.fecha_inicio <= fechaFin.Value);
            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(t => t.categoria_ticket.nombre.Contains(categoria));
            if (!string.IsNullOrEmpty(prioridad))
                query = query.Where(t => t.prioridad == prioridad);
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(t => t.estado == estado);

            var resumen = new DashboardResumenDTO
            {
                TicketsNoAsignados = query.Count(t => t.estado == "No asignado"),
                TicketsEnProgreso = query.Count(t => t.estado == "En Progreso"),
                TicketsResueltos = query.Count(t => t.estado == "Resuelto")
            };

            return Ok(resumen);
        }

    }
}
