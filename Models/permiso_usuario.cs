using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class permiso_usuario
    {
        [Key]
        public int id_permiso_usuario {  get; set; }
        public int id_permiso { get; set; }
        public int id_usuario { get; set; }
    }
}
