using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class ticket
    {
        [Key]
        public int id_ticket { get; set; }
        public string codigo { get; set; }
        public int id_usuario { get; set; }
        public int id_categoria_ticket { get; set; }
        public string servicio { get; set; }
        public string prioridad { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin {  get; set; }
    }
}
