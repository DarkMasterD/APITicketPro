using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class tarea_ticket
    {
        [Key]
        public int id_tarea_ticket { get; set; }
        public int id_ticket {  get; set; }
        public int id_usuario_interno { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin {  get; set; }
    }
}
