using APITicketPro.Models;
using APITicketPro.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace APITicketPro.Controllers
{
    [Route("api/pdf")]
    public class DescargarPDFController : Controller
    {
        private readonly ViewRenderer _viewRenderer;
        private readonly IConverter _converter;
        private readonly DBTicketProContext _context;


        public DescargarPDFController(ViewRenderer viewRenderer, IConverter converter, DBTicketProContext context)
        {
            _viewRenderer = viewRenderer;
            _converter = converter;
            _context = context;
        }



        [HttpGet("ticket/{id}")]
        public async Task<IActionResult> GenerarPDF(int id)
        {
            var ticket = await _context.ticket.FindAsync(id);
            if (ticket == null) return NotFound();

            var dto = new TicketDTO
            {
                IdTicket = ticket.id_ticket,
                Titulo = ticket.titulo,
                Descripcion = ticket.descripcion,
                Fecha = ticket.fecha_inicio
            };

            var html = await _viewRenderer.RenderViewAsync(this.ControllerContext, "PDF/TicketDetalle", dto);

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = { PaperSize = PaperKind.A4 },
                Objects = { new ObjectSettings { HtmlContent = html, WebSettings = { DefaultEncoding = "utf-8" } } }
            };

            var pdfBytes = _converter.Convert(doc);
            return File(pdfBytes, "application/pdf", $"ticket_{id}.pdf");
        }
    }

}
