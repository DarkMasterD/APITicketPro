using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class categoria_ticket
    {
        [Key]
        public int id_categoria_ticket { get; set; }
        public string nombre { get; set; }
        public string descripcion {  get; set; }
    }
}
