using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Examen02.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CombinadoController : ControllerBase
    {
        private readonly HttpClient _http;

        public CombinadoController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
        }

        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetCombined(int codigo)
        {
            try
            {
                // Cambia estas URLs por tus 2 APIs reales
                string facturasUrl = $"https://programacionweb2examen3-production.up.railway.app/api/Facturas/{codigo}";
                string pagosUrl = $"https://programacionweb2examen3-production.up.railway.app/api/Pagos/{codigo}";

                // Llamadas a las APIs
                var facturaResp = await _http.GetAsync(facturasUrl);
                var pagoResp = await _http.GetAsync(pagosUrl);

                if (!facturaResp.IsSuccessStatusCode)
                    return BadRequest(new { error = "Error obteniendo factura" });

                if (!pagoResp.IsSuccessStatusCode)
                    return BadRequest(new { error = "Error obteniendo pago" });

                var factura = await facturaResp.Content.ReadFromJsonAsync<Factura>();
                var pago = await pagoResp.Content.ReadFromJsonAsync<Pago>();

                // Combinar tipo JOIN
                var combinado = new
                {
                    clienteCi = factura.clienteCi,
                    montoTotal = factura.montoTotal,
                    montoPagado = pago?.montoPagado ?? 0,
                    deuda = factura.montoTotal - (pago?.montoPagado ?? 0),
                    pagada = pago != null && pago.montoPagado >= factura.montoTotal
                };

                return Ok(combinado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno", detalle = ex.Message });
            }
        }
    }

    // Modelos DTO
    public class Factura
    {
        public int codigo { get; set; }
        public int clienteCi { get; set; }
        public decimal montoTotal { get; set; }
        public bool pagada { get; set; }
    }

    public class Pago
    {
        public int codigo { get; set; }
        public int facturaCodigo { get; set; }
        public decimal montoPagado { get; set; }
    }
}