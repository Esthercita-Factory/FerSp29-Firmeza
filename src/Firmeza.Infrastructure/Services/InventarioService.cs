using Firmeza.Application.DTOs.Inventarios;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class InventarioService : IInventarioService
{
    private readonly ApplicationDbContext _context;

    public InventarioService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static string DeterminarEstadoStock(int stock)
    {
        if (stock <= 0) return "Agotado";
        if (stock <= 10) return "Bajo Stock";
        return "Stock Normal";
    }

    public async Task<IEnumerable<StockProductoDto>> ObtenerStockGeneralAsync()
    {
        var productos = await _context.Productos.AsNoTracking().ToListAsync();
        return productos.Select(p => new StockProductoDto(
            p.Id,
            p.Codigo,
            p.Nombre,
            p.Stock,
            p.Precio,
            p.Activo,
            DeterminarEstadoStock(p.Stock)
        )).ToList();
    }

    public async Task<StockProductoDto?> ObtenerStockPorProductoAsync(Guid productoId)
    {
        var p = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productoId);
        if (p == null) return null;

        return new StockProductoDto(
            p.Id,
            p.Codigo,
            p.Nombre,
            p.Stock,
            p.Precio,
            p.Activo,
            DeterminarEstadoStock(p.Stock)
        );
    }

    public async Task<(bool Success, string Message, StockProductoDto? Producto)> AjustarStockAsync(AjusteStockDto dto)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null)
            return (false, "El producto especificado no existe.", null);

        switch (dto.TipoAjuste.ToLower())
        {
            case "incrementar":
            case "entrada":
                producto.Stock += dto.Cantidad;
                break;
            case "decrementar":
            case "salida":
                if (producto.Stock < dto.Cantidad)
                    return (false, $"Stock insuficiente. Stock actual: {producto.Stock}, Cantidad solicitada: {dto.Cantidad}.", null);
                producto.Stock -= dto.Cantidad;
                break;
            case "establecer":
            case "fijar":
                if (dto.Cantidad < 0)
                    return (false, "El stock no puede ser un valor negativo.", null);
                producto.Stock = dto.Cantidad;
                break;
            default:
                return (false, "Tipo de ajuste no válido. Use 'Incrementar', 'Decrementar' o 'Establecer'.", null);
        }

        await _context.SaveChangesAsync();

        var respuesta = new StockProductoDto(
            producto.Id,
            producto.Codigo,
            producto.Nombre,
            producto.Stock,
            producto.Precio,
            producto.Activo,
            DeterminarEstadoStock(producto.Stock)
        );

        return (true, "Stock actualizado exitosamente.", respuesta);
    }

    public async Task<ResumenInventarioDto> ObtenerResumenAsync()
    {
        var productos = await _context.Productos.AsNoTracking().ToListAsync();

        return new ResumenInventarioDto(
            TotalProductos: productos.Count,
            TotalStockUnidades: productos.Sum(p => p.Stock),
            ProductosBajoStock: productos.Count(p => p.Stock > 0 && p.Stock <= 10),
            ProductosAgotados: productos.Count(p => p.Stock <= 0),
            ValorizacionTotal: productos.Sum(p => p.Stock * p.Precio)
        );
    }
}
