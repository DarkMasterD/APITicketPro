using APITicketPro.Models;
using APITicketPro.Models.Admin;
using APITicketPro.Services;
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

        private void InsertarProgreso(int id_ticket, int id_usuario_interno, string nombre, string descripcion)
        {
            var progreso = new progreso_ticket
            {
                id_ticket = id_ticket,
                id_usuario_interno = id_usuario_interno,
                nombre = nombre,
                descripcion = descripcion,
                fecha = DateTime.Now
            };

            _context.progreso_ticket.Add(progreso);
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

            if(model.id_usuario_interno.HasValue && !string.IsNullOrEmpty(model.nombre_progreso))
            {
                InsertarProgreso(
                    model.id_ticket,
                    model.id_usuario_interno.Value,
                    model.nombre_progreso,
                    model.descripcion_progreso ?? "Sin descripción"
                );
            }

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
                Titulo = ticket.titulo,
                Descripcion = ticket.descripcion,
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
                    IdTicket = t.id_ticket,
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

        //Detalle del ticket
        [HttpGet("detalle/{id}")]
        public async Task<IActionResult> ObtenerDetalle(int id)
        {
            var ticket = await _context.ticket
                .Include(t => t.usuario)
                    .ThenInclude(u => u.usuario_externo)
                .Include(t => t.usuario.contactos)
                .Include(t => t.categoria_ticket)
                .FirstOrDefaultAsync(t => t.id_ticket == id);

            if (ticket == null) return NotFound();

            var archivo = await _context.ticket_archivo
                .Where(a => a.id_ticket == id)
                .OrderByDescending(a => a.fecha)
                .FirstOrDefaultAsync();

            var model = new TicketDetalleViewModel
            {
                IdTicket = ticket.id_ticket,
                Codigo = ticket.codigo,
                Titulo = ticket.titulo,
                Servicio = ticket.servicio,
                Descripcion = ticket.descripcion,
                Categoria = ticket.categoria_ticket.nombre,
                Prioridad = ticket.prioridad,
                Estado = ticket.estado,
                UrlArchivo = archivo?.url,
                ClienteNombre = ticket.usuario.usuario_externo?.nombre + " " + ticket.usuario.usuario_externo?.apellido,
                ClienteCorreo = ticket.usuario.email,
                ClienteTelefono = ticket.usuario.contactos.FirstOrDefault()?.telefono
            };

            return Ok(model);
        }

        //Actualizar ===============================
        [HttpPost("actualizar-estado")]
        public async Task<IActionResult> ActualizarEstado([FromBody] ActualizarEstadoDTO dto)
        {
            var ticket = await _context.ticket.FindAsync(dto.IdTicket);
            if (ticket == null) return NotFound();

            ticket.estado = dto.NuevoEstado;

            if (dto.NuevoEstado == "Resuelto" && ticket.fecha_fin == default)
            {
                ticket.fecha_fin = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Estado actualizado correctamente" });
        }

        [HttpPost("registrar-progreso")]
        public async Task<IActionResult> RegistrarProgreso([FromBody] ProgresoTicketDTO dto)
        {
            var progreso = new progreso_ticket
            {
                id_ticket = dto.IdTicket,
                id_usuario_interno = 1, // fijo por ahora
                nombre = "Progreso",
                descripcion = dto.Descripcion,
                fecha = DateTime.Now
            };

            _context.progreso_ticket.Add(progreso);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Progreso guardado" });
        }

        [HttpPost("notificar-cliente")]
        public async Task<IActionResult> NotificarCliente([FromBody] NotificarDTO dto, [FromServices] EmailService emailService)
        {
            var ticket = await _context.ticket
                .Include(t => t.usuario)
                .ThenInclude(u => u.usuario_externo)
                .Include(t => t.tareas)
                .ThenInclude(t => t.usuario_interno)
                .FirstOrDefaultAsync(t => t.id_ticket == dto.IdTicket);

            if (ticket == null) return NotFound();

            var correo = ticket.usuario.email;
            var tecnico = ticket.tareas.FirstOrDefault()?.usuario_interno;
            //Este es el cuerpo del correo
            var cuerpo = $@"
                            <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                                <p>Estimado cliente,</p>

                                <p>
                                    Hemos revisado su ticket <strong>“{ticket.titulo}”</strong>, sin embargo, necesitamos información adicional para poder continuar con el proceso de resolución.
                                </p>

                                <p>
                                    Le solicitamos por favor ponerse en contacto con el técnico asignado a su caso para brindarle más detalles:
                                </p>

                                <div style='background-color: #f5f5f5; padding: 12px; border-left: 4px solid #2196f3; margin: 10px 0;'>
                                    <p style='margin: 0;'><strong>Nombre del técnico:</strong> {tecnico?.nombre} {tecnico?.apellido}</p>
                                    <p style='margin: 0;'><strong>Área:</strong> {ticket.servicio}</p>
                                    <p style='margin: 0;'><strong>Estado del ticket:</strong> {ticket.estado}</p>
                                </div>

                                <p>
                                    Agradecemos su comprensión y quedamos atentos a su pronta respuesta para continuar con el seguimiento de su solicitud.
                                </p>

                                <p style='margin-top: 25px;'>Atentamente,<br /><strong>Equipo de Soporte Técnico - TicketPro</strong></p>
                            </div>
                            ";


            await emailService.EnviarNotificacion(correo, "Se requiere más información - TicketPro", cuerpo);

            return Ok(new { mensaje = "Correo enviado" });
        }

        [HttpGet("tickets-cliente/{idUsuario}")]
        public IActionResult TicketsPorCliente(int idUsuario)
        {
            var lista = _context.ticket
                .Include(t => t.categoria_ticket)
                .Where(t => t.id_usuario == idUsuario)
                .Select(t => new
                {
                    t.id_ticket,
                    t.titulo,
                    t.estado,
                    categoria = t.categoria_ticket.nombre,
                    t.prioridad,
                    t.fecha_inicio
                }).ToList();

            return Ok(lista);
        }

        [HttpGet("progreso/{id_ticket}")]
        public IActionResult ObtenerProgresosPorTicket(int id_ticket)
        {
            var progresos = (from p in _context.progreso_ticket
                             join t in _context.usuario_interno on p.id_usuario_interno equals t.id_usuario_interno
                             where p.id_ticket == id_ticket
                             orderby p.fecha
                             select new
                             {
                                 NombreTecnico = t.nombre + " " + t.apellido,
                                 Descripcion = p.descripcion,
                                 Fecha = p.fecha
                             }).ToList();

            return Ok(progresos);
        }



        // ===============================

    }
}
