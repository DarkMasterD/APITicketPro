namespace APITicketPro.Models.Admin
{
    public class ActualizarEstadoDTO
    {
        public int IdTicket { get; set; }
        public string NuevoEstado { get; set; }
    }

    public class ProgresoTicketDTO
    {
        public int IdTicket { get; set; }
        public string Descripcion { get; set; }
    }

    public class NotificarDTO
    {
        public int IdTicket { get; set; }
    }
}
