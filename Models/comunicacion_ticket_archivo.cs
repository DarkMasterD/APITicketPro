using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class comunicacion_ticket_archivo
    {
        [Key]
        public int id_comunicacion_ticket_archivo {  get; set; }
        public int id_comunicacion_ticket { get; set; }
        public int id_ticket_archivo { get; set; }
    }
}
