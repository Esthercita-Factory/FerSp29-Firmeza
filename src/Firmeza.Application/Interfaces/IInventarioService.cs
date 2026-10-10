using Firmeza.Application.DTOs.Inventarios;

namespace Firmeza.Application.Interfaces;

public interface IInventarioService
{
    Task<IEnumerable<StockProductoDto>> ObtenerStockGeneralAsync();
    Task<StockProductoDto?> ObtenerStockPorProductoAsync(Guid productoId);
    Task<(bool Success, string Message, StockProductoDto? Producto)> AjustarStockAsync(AjusteStockDto dto);
    Task<ResumenInventarioDto> ObtenerResumenAsync();
}
