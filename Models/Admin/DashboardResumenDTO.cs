namespace APITicketPro.Models.Admin
{
    public class DashboardResumenDTO
    {
        public int TicketsNoAsignados { get; set; }
        public int TicketsEnProgreso { get; set; }
        public int TicketsCriticos { get; set; }
        public int TicketsResueltos { get; set; }
    }

    public class TicketsAdminViewModel
    {
        public string? Busqueda { get; set; }
        public string? Estado { get; set; }
        public string? Prioridad { get; set; }
        public List<TicketResumenDTO> TicketsFiltrados { get; set; } = new();
    }

}
