using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.MovimientosPedidos;

public record MovimientoDto(
    Guid Id,
    Guid ProductoId,
    string ProductoNombre,
    int Cantidad,
    string TipoMovimiento,
    string Motivo,
    string? DocumentoReferencia,
    DateTime Fecha,
    string? RegistradoPor
);

public record RegistrarMovimientoDto(
    [Required] Guid ProductoId,
    [Required][Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")] int Cantidad,
    [Required] string TipoMovimiento,
    [Required] string Motivo,
    string? DocumentoReferencia
);

public record ItemPedidoDto(
    [Required] Guid ProductoId,
    [Required][Range(1, int.MaxValue)] int Cantidad
);

public record CrearPedidoDto(
    [Required] Guid ClienteId,
    [Required][MinLength(1, ErrorMessage = "El pedido debe contener al menos un producto")] List<ItemPedidoDto> Items,
    string? Observaciones
);

public record DetallePedidoRespuestaDto(
    Guid ProductoId,
    string ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record PedidoRespuestaDto(
    Guid VentaId,
    Guid ClienteId,
    string ClienteNombre,
    DateTime Fecha,
    string Estado,
    decimal Total,
    List<DetallePedidoRespuestaDto> Detalles
);
