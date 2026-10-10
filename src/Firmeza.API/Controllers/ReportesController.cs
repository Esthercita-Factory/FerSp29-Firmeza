using Firmeza.Application.DTOs.Reportes;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _service;

    public ReportesController(IReportesService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardMetricasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard() => Ok(await _service.ObtenerMetricasDashboardAsync());

    [HttpGet("top-productos")]
    [ProducesResponseType(typeof(IEnumerable<TopProductoVendidoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProductos([FromQuery] int top = 5) => Ok(await _service.ObtenerTopProductosAsync(top));

    [HttpGet("ventas-mes")]
    [ProducesResponseType(typeof(IEnumerable<VentasPorMesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentasPorMes() => Ok(await _service.ObtenerVentasPorMesAsync());

    [HttpGet("alertas-stock")]
    [ProducesResponseType(typeof(IEnumerable<AlertaStockBajoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasStock([FromQuery] int umbral = 10) => Ok(await _service.ObtenerAlertasStockBajoAsync(umbral));
}
