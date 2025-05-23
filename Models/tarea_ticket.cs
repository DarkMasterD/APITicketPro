using APITicketPro.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class tarea_ticket
{
    [Key]
    public int id_tarea_ticket { get; set; }

    public int id_ticket { get; set; }

    [ForeignKey("id_ticket")]
    public ticket ticket { get; set; }

    public int id_usuario_interno { get; set; }

    [ForeignKey("id_usuario_interno")]
    public usuario_interno usuario_interno { get; set; }

    public string nombre { get; set; }
    public string descripcion { get; set; }
    public string estado { get; set; }

    public DateTime fecha_inicio { get; set; }
    public DateTime fecha_fin { get; set; }
}
