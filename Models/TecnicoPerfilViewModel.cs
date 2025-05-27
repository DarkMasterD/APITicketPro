namespace APITicketPro.Models
{
    public class TecnicoPerfilViewModel
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Direccion { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<ContactoViewModel>? Contactos { get; set; } 

    }

    public class ContactoViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
    }
}
