using System.ComponentModel.DataAnnotations;

namespace APITicketPro.Models
{
    public class permiso
    {
        [Key]
        public int id_permiso {  get; set; }
        public bool crear_usuario { get; set; }
        public bool editar_usuario { get; set; }
        public bool ver_usuario { get; set; }
        public bool crear_ticket { get; set; }
        public bool editar_ticket { get; set; }
        public bool ver_ticket { get; set; }
        public bool asignacion_ticket { get; set; }
        public bool ver_informes {  get; set; }
        public bool asignacion_tarea_ticket { get; set; }
    }
}
