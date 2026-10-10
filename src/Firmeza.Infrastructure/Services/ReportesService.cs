using Firmeza.Application.DTOs.Reportes;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ReportesService : IReportesService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ReportesService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<DashboardMetricasDto> ObtenerMetricasDashboardAsync()
    {
        var hoyUtc = DateTime.UtcNow.Date;
        var inicioMesUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalIngresosHistoricos = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0;
        var totalIngresosMes = await _context.Ventas
            .Where(v => v.Fecha >= inicioMesUtc)
            .SumAsync(v => (decimal?)v.Total) ?? 0;

        var totalVentas = await _context.Ventas.CountAsync();
        var totalVentasHoy = await _context.Ventas.CountAsync(v => v.Fecha >= hoyUtc);
        var totalClientes = await _context.Clientes.CountAsync();
        var totalProductos = await _context.Productos.CountAsync(p => p.Activo);
        var bajoStock = await _context.Productos.CountAsync(p => p.Stock <= 10);
        var totalUsuarios = await _userManager.Users.CountAsync();

        return new DashboardMetricasDto(
            TotalIngresosHistoricos: totalIngresosHistoricos,
            TotalIngresosMesActual: totalIngresosMes,
            TotalVentasRegistradas: totalVentas,
            TotalVentasHoy: totalVentasHoy,
            TotalClientes: totalClientes,
            TotalProductosActivos: totalProductos,
            TotalTrabajadores: totalUsuarios,
            ProductosConBajoStock: bajoStock
        );
    }

    public async Task<IEnumerable<TopProductoVendidoDto>> ObtenerTopProductosAsync(int top = 5)
    {
        var detalles = await _context.Detalles
            .AsNoTracking()
            .Include(d => d.Producto)
            .ToListAsync();

        return detalles
            .GroupBy(d => new { d.ProductoId, Codigo = d.Producto?.Codigo ?? string.Empty, Nombre = d.Producto?.Nombre ?? string.Empty })
            .Select(g => new TopProductoVendidoDto(
                g.Key.ProductoId,
                g.Key.Codigo,
                g.Key.Nombre,
                g.Sum(x => x.Cantidad),
                g.Sum(x => x.Subtotal)
            ))
            .OrderByDescending(x => x.UnidadesVendidas)
            .Take(top)
            .ToList();
    }

    public async Task<IEnumerable<VentasPorMesDto>> ObtenerVentasPorMesAsync()
    {
        var ventas = await _context.Ventas.AsNoTracking().ToListAsync();

        return ventas
            .GroupBy(v => v.Fecha.ToString("yyyy-MM"))
            .Select(g => new VentasPorMesDto(
                g.Key,
                g.Count(),
                g.Sum(v => v.Total)
            ))
            .OrderByDescending(x => x.AnioMes)
            .Take(12)
            .ToList();
    }

    public async Task<IEnumerable<AlertaStockBajoDto>> ObtenerAlertasStockBajoAsync(int umbral = 10)
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Stock <= umbral)
            .OrderBy(p => p.Stock)
            .ToListAsync();

        return productos.Select(p => new AlertaStockBajoDto(
            p.Id,
            p.Codigo,
            p.Nombre,
            p.Stock,
            p.Precio
        )).ToList();
    }
}
