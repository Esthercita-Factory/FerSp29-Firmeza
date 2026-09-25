using Firmeza.Infrastructure.Persistence;
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

        return services;
    }
}