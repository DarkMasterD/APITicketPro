using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class usuario
    {
        [Key]
        public int id_usuario { get; set; }
        public string nombre_usuario { get; set; }
        public string email { get; set; }
        public string contrasenia { get; set; }
        public char tipo_usuario { get; set; }
        public bool estado { get; set; }
        public DateTime fecha_registro { get; set; }

    }
}
