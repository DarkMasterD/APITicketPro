using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITicketPro.Models
{
    public class contacto_usuario
    {
        [Key]
        public int id_contacto_usuario { get; set; }
        [ForeignKey("usuario")]
        public int id_usuario { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public usuario usuario { get; set; }

    }
}
