using Firmeza.Application.DTOs.Inventarios;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InventariosController : ControllerBase
{
    private readonly IInventarioService _service;

    public InventariosController(IInventarioService service)
    {
        _service = service;
    }

    [HttpGet("stock")]
    [ProducesResponseType(typeof(IEnumerable<StockProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerStockGeneral() => Ok(await _service.ObtenerStockGeneralAsync());

    [HttpGet("stock/{productoId:guid}")]
    [ProducesResponseType(typeof(StockProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerStockPorProducto(Guid productoId)
    {
        var stock = await _service.ObtenerStockPorProductoAsync(productoId);
        return stock == null ? NotFound(new { message = "Producto no encontrado en inventario." }) : Ok(stock);
    }

    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenInventarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerResumen() => Ok(await _service.ObtenerResumenAsync());

    [HttpPost("ajustar-stock")]
    [ProducesResponseType(typeof(StockProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AjustarStock([FromBody] AjusteStockDto dto)
    {
        var (success, message, producto) = await _service.AjustarStockAsync(dto);
        if (!success) return BadRequest(new { message });

        return Ok(new { message, producto });
    }
}
