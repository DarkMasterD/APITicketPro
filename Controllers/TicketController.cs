using APITicketPro.Models;
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
        [Route("ListarTareasTicket")]
        public IActionResult ListarTareasTicket(int id_ticket)
        {

        }
    }
}
