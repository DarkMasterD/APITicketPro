namespace APITicketPro.Models
{
    public class TecnicoPerfilViewModel
    {
        public string NombreCompleto { get; set; } // Combina nombre + apellido
        public string Direccion { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<ContactoViewModel> Contactos { get; set; } // Nuevo para múltiples contactos
    }

    public class ContactoViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
    }
}
