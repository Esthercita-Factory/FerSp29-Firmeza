using Firmeza.Domain.Entities;
using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Firmeza.UnitTests;

public class PersistenceTests
{
    private const string ConnectionString = "Host=localhost;Port=5433;Database=firmeza_db;Username=postgres;Password=postgres";

    [Fact]
    public void ApplicationDbContext_Model_ShouldContainRequiredEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);

        // Assert
        Assert.NotNull(context.Clientes);
        Assert.NotNull(context.Productos);
        Assert.NotNull(context.Ventas);
        Assert.NotNull(context.Detalles);

        var model = context.Model;
        Assert.NotNull(model.FindEntityType(typeof(Cliente)));
        Assert.NotNull(model.FindEntityType(typeof(Producto)));
        Assert.NotNull(model.FindEntityType(typeof(Venta)));
        Assert.NotNull(model.FindEntityType(typeof(Detalle)));
    }

    [Fact]
    public void Entities_ShouldInstantiateAndStoreDataCorrectly()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ventaId = Guid.NewGuid();
        var detalleId = Guid.NewGuid();

        var cliente = new Cliente
        {
            Id = clienteId,
            Nombre = "Juan Pérez",
            Correo = "juan@firmeza.com",
            Telefono = "3001234567",
            Direccion = "Calle 10 # 20",
            FechaRegistro = DateTime.UtcNow
        };

        var producto = new Producto
        {
            Id = productoId,
            Codigo = "PROD-01",
            Nombre = "Cemento Gris",
            Precio = 32000m,
            Stock = 50,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var venta = new Venta
        {
            Id = ventaId,
            ClienteId = cliente.Id,
            Cliente = cliente,
            Fecha = DateTime.UtcNow,
            Total = 64000m,
            Estado = "Completada"
        };

        var detalle = new Detalle
        {
            Id = detalleId,
            VentaId = venta.Id,
            Venta = venta,
            ProductoId = producto.Id,
            Producto = producto,
            Cantidad = 2,
            PrecioUnitario = 32000m,
            Subtotal = 64000m
        };

        // Assert
        Assert.Equal("Juan Pérez", cliente.Nombre);
        Assert.Equal("Cemento Gris", producto.Nombre);
        Assert.Equal(64000m, venta.Total);
        Assert.Equal(2, detalle.Cantidad);
        Assert.Equal(detalle.VentaId, venta.Id);
        Assert.Equal(detalle.ProductoId, producto.Id);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterApplicationDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public async Task PostgreSQL_Database_ShouldConnectAndQuerySeedData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);

        var productosCount = await context.Productos.CountAsync();
        Assert.True(productosCount >= 3);

        var clientesCount = await context.Clientes.CountAsync();
        Assert.True(clientesCount >= 2);
    }
}
