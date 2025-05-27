using APITicketPro.Models;
using APITicketPro.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace APITicketPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly DBTicketProContext _context;
        public RolController(DBTicketProContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolDTO>>> GetRoles()
        {
            var roles = await _context.rol
                .Select(r => new RolDTO
                {
                    IdRol = r.id_rol,
                    Nombre = r.nombre
                }).ToListAsync();

            return Ok(roles);
        }
    }
}
