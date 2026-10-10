using Firmeza.Application.DTOs.MovimientosPedidos;

namespace Firmeza.Application.Interfaces;

public interface IMovimientosPedidosService
{
    Task<(bool Success, string Message, MovimientoDto? Movimiento)> RegistrarMovimientoAsync(RegistrarMovimientoDto dto, string? usuario);
    Task<IEnumerable<MovimientoDto>> ObtenerMovimientosAsync();
    Task<(bool Success, string Message, PedidoRespuestaDto? Pedido)> CrearPedidoAsync(CrearPedidoDto dto);
    Task<IEnumerable<PedidoRespuestaDto>> ObtenerPedidosAsync();
    Task<PedidoRespuestaDto?> ObtenerPedidoPorIdAsync(Guid id);
}
