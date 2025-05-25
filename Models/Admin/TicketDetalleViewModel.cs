namespace APITicketPro.Models.Admin
{
    public class TicketDetalleViewModel
    {
        public int IdTicket { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string Servicio { get; set; }
        public string Categoria { get; set; }
        public string Prioridad { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public string Estado { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteCorreo { get; set; }
        public string ClienteTelefono { get; set; }
    }
}
