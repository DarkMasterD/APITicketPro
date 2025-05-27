namespace APITicketPro.Models.Admin
{
    public class UsuarioListadoDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string Empresa { get; set; }
    }
    public class RolDTO
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }
    }

    public class UsuarioExternoViewModel
    {
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Usuario { get; set; }
        public string? Email { get; set; }
        public string Empresa { get; set; }
        public string Contrasena { get; set; }

        public string? TipoUsuario { get; set; } = "E";

    }
    public class CrearContactoDTO
    {
        public int id_usuario { get; set; }
        public string? email { get; set; }
        public string? telefono { get; set; }
    }

}
