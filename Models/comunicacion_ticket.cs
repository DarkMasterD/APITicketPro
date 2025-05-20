using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class comunicacion_ticket
    {
        [Key]
        public int id_comunicacion_ticket {  get; set; }
        public int id_ticket { get; set; }
        public int id_usuario { get; set; }
        public string comentario { get; set; }
        public DateTime fecha { get; set; }
    }
}
