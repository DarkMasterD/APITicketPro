namespace APITicketPro.Models.Admin
{
    public class UsuarioEditarViewModel
    {
        public int? IdUsuario { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public string Contrasena { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        // Solo externos
        public string? Empresa { get; set; }

        // Solo internos
        public string? Direccion { get; set; }
        public string? Dui { get; set; }
        public int? IdRol { get; set; }

        public string TipoUsuario { get; set; }  // "E" o "I"
    }

}
