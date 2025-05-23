using APITicketPro.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ticket
{
    [Key]
    public int id_ticket { get; set; }
    public string codigo { get; set; }
    public int id_usuario { get; set; }

    [ForeignKey("id_usuario")]
    public usuario usuario { get; set; }

    public int id_categoria_ticket { get; set; }
    public string servicio { get; set; }
    public string prioridad { get; set; }
    public string titulo { get; set; }
    public string descripcion { get; set; }
    public string estado { get; set; }

    public DateTime fecha_inicio { get; set; }
    public DateTime? fecha_fin { get; set; } //Le agrege si es null por que si no esta cerrado no tiene fecha

    public ICollection<tarea_ticket> tareas { get; set; }
}

public class ticketDetalleDTO
{
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
    public List<TareaTicketItem> Tareas { get; set; }
}

