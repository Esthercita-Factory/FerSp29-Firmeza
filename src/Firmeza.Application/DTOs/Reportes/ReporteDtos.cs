namespace Firmeza.Application.DTOs.Reportes;

public record DashboardMetricasDto(
    decimal TotalIngresosHistoricos,
    decimal TotalIngresosMesActual,
    int TotalVentasRegistradas,
    int TotalVentasHoy,
    int TotalClientes,
    int TotalProductosActivos,
    int TotalTrabajadores,
    int ProductosConBajoStock
);

public record TopProductoVendidoDto(
    Guid ProductoId,
    string Codigo,
    string Nombre,
    int UnidadesVendidas,
    decimal TotalRecaudado
);

public record VentasPorMesDto(
    string AnioMes,
    int CantidadVentas,
    decimal TotalMonto
);

public record AlertaStockBajoDto(
    Guid ProductoId,
    string Codigo,
    string Nombre,
    int StockActual,
    decimal Precio
);
