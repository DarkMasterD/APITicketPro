namespace APITicketPro.Models.Admin
{
    public class CrearTareaDTO
    {
        public int IdTicket { get; set; }
        public int IdTecnico { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
    }
}
