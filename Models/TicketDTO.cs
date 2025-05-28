namespace APITicketPro.Models
{
    public class TicketDTO
    {
        public int IdTicket { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
