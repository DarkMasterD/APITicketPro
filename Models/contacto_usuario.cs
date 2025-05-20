using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class contacto_usuario
    {
        [Key]
        public int id_contacto_usuario { get; set; }
        public int id_usuario { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
    }
}
