using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITicketPro.Models
{
    public class usuario_interno
    {
        [Key]
        public int id_usuario_interno { get; set; }
        public int id_usuario { get; set; }
        [ForeignKey("id_usuario")]
        public usuario usuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string direccion {  get; set; }
        public string dui {  get; set; }
        public int id_rol {  get; set; }
        [ForeignKey("id_rol")]
        public rol rol { get; set; }
    }
}
