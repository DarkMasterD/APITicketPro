using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class progreso_tarea_ticket
    {
        [Key]
        public int id_progreso_tarea_ticket {  get; set; }
        public int id_tarea_ticket { get; set; }
        public string comentario { get; set; }
        public DateTime fecha { get; set; }
    }
}
