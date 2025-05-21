namespace APITicketPro.Models.Admin
{
    public class TicketResumenDTO
    {
        public string Titulo { get; set; }
        public string Cliente { get; set; }
        public string Tecnico { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public DateTime Fecha { get; set; }
    }
}
