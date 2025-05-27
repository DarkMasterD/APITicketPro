using APITicketPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : Controller
    {
        private readonly DBTicketProContext _context;

        public CategoriaController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<categoria_ticket>>> GetCategorias()
        {
            var categorias = await _context.categoria_ticket.ToListAsync();
            return Ok(categorias);
        }
    }
}
