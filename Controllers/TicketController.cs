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

        [HttpGet("generar-codigo")]
        public async Task<IActionResult> GenerarCodigo([FromQuery] string prioridad, [FromQuery] char tipoUsuario)
        {
            string codigoGenerado = string.Empty;

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "GenerarCodigoTicket";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@Prioridad";
                    param1.Value = prioridad;
                    command.Parameters.Add(param1);

                    var param2 = command.CreateParameter();
                    param2.ParameterName = "@TipoUsuario";
                    param2.Value = tipoUsuario;
                    command.Parameters.Add(param2);

                    var output = command.CreateParameter();
                    output.ParameterName = "@CodigoGenerado";
                    output.DbType = System.Data.DbType.String;
                    output.Size = 50;
                    output.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(output);

                    await command.ExecuteNonQueryAsync();
                    codigoGenerado = output.Value.ToString();
                }
            }

            return Ok(new { codigo = codigoGenerado });
        }
        // crear ticket 
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ticket nuevoTicket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ticket.Add(nuevoTicket);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket creado", id = nuevoTicket.id_ticket });
        }

        // Agregar archivo por ticket
        [HttpPost("subir-archivo")]
        public async Task<IActionResult> SubirArchivo(IFormFile archivo, [FromForm] int id_ticket)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo no válido");

            var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "archivos_ticket");

            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Guardar en la base de datos
            var archivoTicket = new ticket_archivo
            {
                id_ticket = id_ticket,
                url = "/archivos_ticket/" + nombreArchivo, // se guarda la ruta relativa
                fecha = DateTime.Now
            };

            _context.ticket_archivo.Add(archivoTicket);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Archivo subido exitosamente" });
        }

    }
}
