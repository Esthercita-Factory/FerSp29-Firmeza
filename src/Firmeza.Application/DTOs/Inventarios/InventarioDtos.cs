using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Inventarios;

public record StockProductoDto(
    Guid ProductoId,
    string Codigo,
    string Nombre,
    int StockActual,
    decimal Precio,
    bool Activo,
    string EstadoStock
);

public record AjusteStockDto(
    [Required] Guid ProductoId,
    [Required] int Cantidad,
    [Required] string TipoAjuste,
    string? Motivo
);

public record ResumenInventarioDto(
    int TotalProductos,
    int TotalStockUnidades,
    int ProductosBajoStock,
    int ProductosAgotados,
    decimal ValorizacionTotal
);
