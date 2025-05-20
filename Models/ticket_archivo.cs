using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class ticket_archivo
    {
        [Key]
        public int id_ticket_archivo { get; set; }
        public int id_ticket { get; set; }
        public string url { get; set; }
        public DateTime fecha { get; set; }
    }
}
