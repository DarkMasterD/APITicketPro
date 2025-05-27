using APITicketPro.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class ticket
{
    [Key]
    public int id_ticket { get; set; }
    public string codigo { get; set; }
    public int id_usuario { get; set; }

    [ForeignKey("id_usuario")]
    public usuario? usuario { get; set; }

    public int id_categoria_ticket { get; set; }
    public string servicio { get; set; }
    public string prioridad { get; set; }
    public string titulo { get; set; }
    public string descripcion { get; set; }
    public string estado { get; set; }

    public DateTime fecha_inicio { get; set; }
    public DateTime? fecha_fin { get; set; } //Le agrege si es null por que si no esta cerrado no tiene fecha

    public ICollection<tarea_ticket> tareas { get; set; }
    [ForeignKey("id_categoria_ticket")]
    public categoria_ticket? categoria_ticket { get; set; }
}

public class ticketDetalleDTO
{
    public int id_ticket { get; set; }
    public string Titulo { get; set; }
    public string Servicio { get; set; }
    public string Descripcion { get; set; }
    public string Cliente_Afectado { get; set; }
    public string Correo { get; set; }
    public string Telefono { get; set; }
    public string Url_Archivo { get; set; }
    public string Codigo { get; set; }
    public string Categoria_Ticket { get; set; }
    public string Prioridad { get; set; }
    public string Estado { get; set; }
}

public class ticketEstadoUpdateModel
{
    public int id_ticket { get; set; }
    public string estado { get; set; }
    // Campos para registrar progreso
    public int? id_usuario_interno { get; set; } 
    public string? nombre_progreso { get; set; }
    public string? descripcion_progreso { get; set; }
}

public class TareaTicketItem
{
    public string Nombre { get; set; }
    public string Estado { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }  // Puede ser null
    public string UsuarioAsignado { get; set; } // Nombre completo del usuario
}

public class tareaTicketViewModel
{
    public int IdTicket { get; set; }
    public string Codigo { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    // NUEVO: para mostrar el combobox
    public List<SelectListItem> Tecnicos { get; set; }
    // NUEVO: para capturar el técnico seleccionado
    public string IdTecnicoSeleccionado { get; set; }
    public List<TareaTicketItem> Tareas { get; set; }
}

public class NuevaTareaDto
{
    public int IdTicket { get; set; }
    public int IdUsuarioInterno { get; set; }  // Técnico asignado
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string Estado { get; set; }  // 'Asignada', 'En progreso', etc.
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }  // Opcional
}

