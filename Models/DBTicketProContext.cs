using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Models
{
    public class DBTicketProContext : DbContext
    {
        public DBTicketProContext(DbContextOptions<DBTicketProContext> options) : base(options)
        {

        }
    }
}
