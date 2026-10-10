using Firmeza.Application.DTOs.Reportes;

namespace Firmeza.Application.Interfaces;

public interface IReportesService
{
    Task<DashboardMetricasDto> ObtenerMetricasDashboardAsync();
    Task<IEnumerable<TopProductoVendidoDto>> ObtenerTopProductosAsync(int top = 5);
    Task<IEnumerable<VentasPorMesDto>> ObtenerVentasPorMesAsync();
    Task<IEnumerable<AlertaStockBajoDto>> ObtenerAlertasStockBajoAsync(int umbral = 10);
}
