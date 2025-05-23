using APITicketPro.Models;
using APITicketPro.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

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


        [HttpGet]
        [Route("ListarDetalle")]
        public IActionResult ListarDetalle(int idTicket)
        {

            var datosDetalle = (from t in _context.ticket
                                join u in _context.usuario on t.id_usuario equals u.id_usuario

                                // Left join usuario_interno
                                join ui in _context.usuario_interno on u.id_usuario equals ui.id_usuario into uiGroup
                                from ui in uiGroup.DefaultIfEmpty()

                                    // Left join usuario_externo
                                join ue in _context.usuario_externo on u.id_usuario equals ue.id_usuario into ueGroup
                                from ue in ueGroup.DefaultIfEmpty()

                                    // Left join contacto_usuario
                                join cu in _context.contacto_usuario on u.id_usuario equals cu.id_usuario into contactoJoin
                                from cu in contactoJoin.DefaultIfEmpty()

                                    // Left join ticket_archivo
                                join ta in _context.ticket_archivo on t.id_ticket equals ta.id_ticket into archivoJoin
                                from ta in archivoJoin.DefaultIfEmpty()

                                join ct in _context.categoria_ticket on t.id_categoria_ticket equals ct.id_categoria_ticket

                                where t.id_ticket == idTicket

                                select new ticketDetalleDTO
                                {
                                    Titulo = t.titulo,
                                    Servicio = t.servicio,
                                    Descripcion = t.descripcion,

                                    // Priorizar usuario_interno, si no existe usar usuario_externo, si no 'Sin usuario'
                                    Cliente_Afectado = ui != null ? (ui.nombre + " " + ui.apellido) :
                                                     ue != null ? (ue.nombre + " " + ue.apellido) :
                                                     "Sin usuario",

                                    Correo = u.email,
                                    Telefono = cu != null ? cu.telefono : "No registrado",
                                    Url_Archivo = ta != null ? ta.url : "Sin archivo",
                                    Codigo = t.codigo,
                                    Categoria_Ticket = ct.nombre,
                                    Prioridad = t.prioridad,
                                    Estado = t.estado
                                });

            return Ok(datosDetalle.ToList());
        }

        [HttpPost]
        [Route("actualizarTicket")]
        public IActionResult ActualizarTicket([FromBody] ticketEstadoUpdateModel model)
        {
            if (model == null || model.id_ticket <= 0 || string.IsNullOrEmpty(model.estado))
            {
                return BadRequest("Datos inválidos");
            }

            var ticket = _context.ticket.FirstOrDefault(t => t.id_ticket == model.id_ticket);
            if (ticket == null)
            {
                return NotFound("Ticket no encontrado");
            }

            ticket.estado = model.estado;
            _context.SaveChanges();

            return Ok("Ticket actualizado correctamente");
        }

        [HttpGet]
        [Route("VerTareasDelTicket")]
        public IActionResult VerTareasDelTicket(int idTicket)
        {
            // Verificar si el ticket existe
            var ticket = (from t in _context.ticket
                          where t.id_ticket == idTicket
                          select t).FirstOrDefault();
            if (ticket == null)
                return NotFound();

            // Unimos tarea_ticket con usuario_interno
            var tareas = (from tarea in _context.tarea_ticket
                          join usuario in _context.usuario_interno
                          on tarea.id_usuario_interno equals usuario.id_usuario_interno
                          where tarea.id_ticket == idTicket
                          select new TareaTicketItem
                          {
                              Nombre = tarea.nombre,
                              Estado = tarea.estado,
                              FechaInicio = tarea.fecha_inicio,
                              UsuarioAsignado = usuario.nombre + " " + usuario.apellido
                          }).ToList();

            // Construir el ViewModel
            var viewModel = new tareaTicketViewModel
            {
                IdTicket = ticket.id_ticket,
                Codigo = ticket.codigo,
                Tareas = tareas
            };

            return Ok(viewModel);
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
        
    }
}
