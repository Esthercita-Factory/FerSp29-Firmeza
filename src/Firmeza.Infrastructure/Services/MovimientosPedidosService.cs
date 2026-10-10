using System.Collections.Concurrent;
using Firmeza.Application.DTOs.MovimientosPedidos;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class MovimientosPedidosService : IMovimientosPedidosService
{
    private readonly ApplicationDbContext _context;
    private static readonly ConcurrentBag<MovimientoDto> _movimientos = new();

    public MovimientosPedidosService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, MovimientoDto? Movimiento)> RegistrarMovimientoAsync(RegistrarMovimientoDto dto, string? usuario)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null)
            return (false, "Producto no encontrado.", null);

        var esEntrada = string.Equals(dto.TipoMovimiento, "Entrada", StringComparison.OrdinalIgnoreCase);
        var esSalida = string.Equals(dto.TipoMovimiento, "Salida", StringComparison.OrdinalIgnoreCase);

        if (!esEntrada && !esSalida)
            return (false, "El tipo de movimiento debe ser 'Entrada' o 'Salida'.", null);

        if (esSalida && producto.Stock < dto.Cantidad)
            return (false, $"Stock insuficiente. Disponible: {producto.Stock}.", null);

        if (esEntrada)
            producto.Stock += dto.Cantidad;
        else
            producto.Stock -= dto.Cantidad;

        await _context.SaveChangesAsync();

        var movimiento = new MovimientoDto(
            Id: Guid.NewGuid(),
            ProductoId: producto.Id,
            ProductoNombre: producto.Nombre,
            Cantidad: dto.Cantidad,
            TipoMovimiento: esEntrada ? "Entrada" : "Salida",
            Motivo: dto.Motivo,
            DocumentoReferencia: dto.DocumentoReferencia,
            Fecha: DateTime.UtcNow,
            RegistradoPor: usuario ?? "Sistema"
        );

        _movimientos.Add(movimiento);
        return (true, "Movimiento registrado exitosamente.", movimiento);
    }

    public Task<IEnumerable<MovimientoDto>> ObtenerMovimientosAsync()
    {
        return Task.FromResult<IEnumerable<MovimientoDto>>(_movimientos.OrderByDescending(m => m.Fecha).ToList());
    }

    public async Task<(bool Success, string Message, PedidoRespuestaDto? Pedido)> CrearPedidoAsync(CrearPedidoDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null)
            return (false, "El cliente no existe.", null);

        var venta = new Venta
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Fecha = DateTime.UtcNow,
            Estado = "Completada",
            Total = 0
        };

        var detallesRespuesta = new List<DetallePedidoRespuestaDto>();
        decimal totalVenta = 0;

        foreach (var item in dto.Items)
        {
            var prod = await _context.Productos.FindAsync(item.ProductoId);
            if (prod == null)
                return (false, $"Producto con ID {item.ProductoId} no encontrado.", null);

            if (prod.Stock < item.Cantidad)
                return (false, $"Stock insuficiente para '{prod.Nombre}'. Disponible: {prod.Stock}.", null);

            prod.Stock -= item.Cantidad;
            var subtotal = item.Cantidad * prod.Precio;
            totalVenta += subtotal;

            var detalle = new Detalle
            {
                Id = Guid.NewGuid(),
                VentaId = venta.Id,
                ProductoId = prod.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = prod.Precio,
                Subtotal = subtotal
            };

            venta.Detalles.Add(detalle);
            detallesRespuesta.Add(new DetallePedidoRespuestaDto(prod.Id, prod.Nombre, item.Cantidad, prod.Precio, subtotal));
        }

        venta.Total = totalVenta;
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        var pedidoRespuesta = new PedidoRespuestaDto(
            venta.Id,
            cliente.Id,
            cliente.Nombre,
            venta.Fecha,
            venta.Estado,
            venta.Total,
            detallesRespuesta
        );

        return (true, "Pedido registrado exitosamente.", pedidoRespuesta);
    }

    public async Task<IEnumerable<PedidoRespuestaDto>> ObtenerPedidosAsync()
    {
        var ventas = await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return ventas.Select(v => new PedidoRespuestaDto(
            v.Id,
            v.ClienteId,
            v.Cliente?.Nombre ?? "N/A",
            v.Fecha,
            v.Estado,
            v.Total,
            v.Detalles.Select(d => new DetallePedidoRespuestaDto(
                d.ProductoId,
                d.Producto?.Nombre ?? "N/A",
                d.Cantidad,
                d.PrecioUnitario,
                d.Subtotal
            )).ToList()
        )).ToList();
    }

    public async Task<PedidoRespuestaDto?> ObtenerPedidoPorIdAsync(Guid id)
    {
        var v = await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (v == null) return null;

        return new PedidoRespuestaDto(
            v.Id,
            v.ClienteId,
            v.Cliente?.Nombre ?? "N/A",
            v.Fecha,
            v.Estado,
            v.Total,
            v.Detalles.Select(d => new DetallePedidoRespuestaDto(
                d.ProductoId,
                d.Producto?.Nombre ?? "N/A",
                d.Cantidad,
                d.PrecioUnitario,
                d.Subtotal
            )).ToList()
        );
    }
}
