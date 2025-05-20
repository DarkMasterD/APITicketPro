using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class progreso_ticket
    {
        [Key]
        public int id_progreso_ticket { get; set; }
        public int id_ticket {  get; set; }
        public int id_usuario_interno { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha { get; set; }
    }
}
