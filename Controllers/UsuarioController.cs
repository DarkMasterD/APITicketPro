using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITicketPro.Models;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller
    {
        private readonly DBTicketProContext _context;

        public UsuarioController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
        {
            var usuarios = await _context.usuario
                .Include(u => u.contactos)
                .Select(u => new
                {
                    id_usuario = u.id_usuario,
                    nombre_usuario = u.nombre_usuario,
                    email = u.email,
                    tipo_usuario = u.tipo_usuario,
                    telefono = u.contactos
                        .OrderByDescending(c => c.id_contacto_usuario)
                        .FirstOrDefault().telefono
                })
                .ToListAsync();


            return Ok(usuarios);
        }

    }
}
