using APITicketPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITicketPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {
        private readonly DBTicketProContext _context;

        public TicketController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tickets = await _context.ticket.ToListAsync();
            return Ok(tickets);
        }
    }
}
