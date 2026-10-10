using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var config = configuration ?? serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection") 
                ?? "Host=localhost;Port=5433;Database=firmeza_db;Username=postgres;Password=postgres";
            
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IAdminEmpleadoService, AdminEmpleadoService>();
        services.AddScoped<IVehiculoService, VehiculoService>();
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<IMovimientosPedidosService, MovimientosPedidosService>();
        services.AddScoped<IReportesService, ReportesService>();

        return services;
    }
}