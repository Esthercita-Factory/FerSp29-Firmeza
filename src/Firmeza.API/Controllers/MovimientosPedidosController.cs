using System.Security.Claims;
using Firmeza.Application.DTOs.MovimientosPedidos;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MovimientosPedidosController : ControllerBase
{
    private readonly IMovimientosPedidosService _service;

    public MovimientosPedidosController(IMovimientosPedidosService service)
    {
        _service = service;
    }

    [HttpPost("movimientos")]
    [ProducesResponseType(typeof(MovimientoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoDto dto)
    {
        var usuarioEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (success, message, movimiento) = await _service.RegistrarMovimientoAsync(dto, usuarioEmail);

        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, movimiento });
    }

    [HttpGet("movimientos")]
    [ProducesResponseType(typeof(IEnumerable<MovimientoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerMovimientos() => Ok(await _service.ObtenerMovimientosAsync());

    [HttpPost("pedidos")]
    [ProducesResponseType(typeof(PedidoRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto dto)
    {
        var (success, message, pedido) = await _service.CrearPedidoAsync(dto);
        if (!success) return BadRequest(new { message });

        return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = pedido!.VentaId }, pedido);
    }

    [HttpGet("pedidos")]
    [ProducesResponseType(typeof(IEnumerable<PedidoRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPedidos() => Ok(await _service.ObtenerPedidosAsync());

    [HttpGet("pedidos/{id:guid}")]
    [ProducesResponseType(typeof(PedidoRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPedidoPorId(Guid id)
    {
        var pedido = await _service.ObtenerPedidoPorIdAsync(id);
        return pedido == null ? NotFound(new { message = $"Pedido con ID '{id}' no encontrado." }) : Ok(pedido);
    }
}
