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
    public DateTime fecha_fin { get; set; }

    public ICollection<tarea_ticket> tareas { get; set; }
}
