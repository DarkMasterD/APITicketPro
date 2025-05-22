using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Models
{
    public class DBTicketProContext : DbContext
    {
        public DBTicketProContext(DbContextOptions<DBTicketProContext> options) : base(options)
        {

        }
        public DbSet<ticket> ticket { get; set; }
        public DbSet<categoria_ticket> categoria_ticket { get; set; }
        public DbSet<comunicacion_ticket> comunicacion_ticket { get; set; }
        public DbSet<comunicacion_ticket_archivo> comunicacion_ticket_archivo { get; set; }
        public DbSet<contacto_usuario> contacto_usuario { get; set; }
        public DbSet<permiso> permiso { get; set; }
        public DbSet<permiso_usuario> permiso_usuario { get; set; }
        public DbSet<progreso_tarea_ticket> progreso_tarea_ticket { get; set; }
        public DbSet<progreso_ticket> progreso_ticket { get; set; }
        public DbSet<rol> rol { get; set; }
        public DbSet<tarea_ticket> tarea_ticket { get; set; }
        public DbSet<ticket_archivo> ticket_archivo { get; set; }
        public DbSet<usuario> usuario { get; set; }
        public DbSet<usuario_externo> usuario_externo { get; set; }
        public DbSet<usuario_interno> usuario_interno { get; set; }

    }
}
