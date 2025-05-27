namespace APITicketPro.Models.Admin
{
    public class CrearTareaDTO
    {
        public int IdTicket { get; set; }
        public int IdTecnico { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
    }

    public class TecnicoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }


}
