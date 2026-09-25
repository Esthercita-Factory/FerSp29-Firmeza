using Firmeza.Application;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Firmeza.UnitTests;

public class DatabaseIntegrationTests
{
    private const string ConnectionString = "Host=localhost;Port=5433;Database=firmeza_db;Username=postgres;Password=postgres";

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task PostgreSQL_CRUD_Cliente_ShouldWorkCorrectly()
    {
        using var context = CreateContext();
        var uniqueEmail = $"test_{Guid.NewGuid():N}@firmeza.com";

        // 1. Create
        var cliente = new Cliente
        {
            Nombre = "Cliente Test Automatizado",
            Correo = uniqueEmail,
            Telefono = "3000000000",
            Direccion = "Avenida Siempre Viva 123",
            FechaRegistro = DateTime.UtcNow
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        Assert.NotEqual(Guid.Empty, cliente.Id);

        // 2. Read
        var clienteDb = await context.Clientes.FirstOrDefaultAsync(c => c.Id == cliente.Id);
        Assert.NotNull(clienteDb);
        Assert.Equal("Cliente Test Automatizado", clienteDb.Nombre);
        Assert.Equal(uniqueEmail, clienteDb.Correo);

        // 3. Update
        clienteDb.Telefono = "3111111111";
        clienteDb.Direccion = "Nueva Direccion 456";
        await context.SaveChangesAsync();

        var clienteUpdated = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cliente.Id);
        Assert.NotNull(clienteUpdated);
        Assert.Equal("3111111111", clienteUpdated.Telefono);
        Assert.Equal("Nueva Direccion 456", clienteUpdated.Direccion);

        // 4. Delete
        context.Clientes.Remove(clienteDb);
        await context.SaveChangesAsync();

        var clienteDeleted = await context.Clientes.FirstOrDefaultAsync(c => c.Id == cliente.Id);
        Assert.Null(clienteDeleted);
    }

    [Fact]
    public async Task PostgreSQL_CRUD_VentaConDetalles_ShouldHandleRelationshipsAndCascadeDelete()
    {
        using var context = CreateContext();

        // 1. Arrange: Cliente y Producto
        var uniqueEmail = $"cliente_venta_{Guid.NewGuid():N}@firmeza.com";
        var cliente = new Cliente
        {
            Nombre = "Comprador de Prueba",
            Correo = uniqueEmail,
            Telefono = "3200000000",
            Direccion = "Calle Prueba 789",
            FechaRegistro = DateTime.UtcNow
        };
        context.Clientes.Add(cliente);

        var uniqueCode = $"PRD-{Guid.NewGuid():N}"[..12];
        var producto = new Producto
        {
            Codigo = uniqueCode,
            Nombre = "Producto de Prueba",
            Descripcion = "Descripción de prueba",
            Precio = 15000.00m,
            Stock = 100,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        // 2. Create Venta con Detalle
        var venta = new Venta
        {
            ClienteId = cliente.Id,
            Fecha = DateTime.UtcNow,
            Total = 30000.00m,
            Estado = "Pendiente"
        };
        context.Ventas.Add(venta);
        await context.SaveChangesAsync();

        var detalle = new Detalle
        {
            VentaId = venta.Id,
            ProductoId = producto.Id,
            Cantidad = 2,
            PrecioUnitario = 15000.00m,
            Subtotal = 30000.00m
        };
        context.Detalles.Add(detalle);
        await context.SaveChangesAsync();

        // 3. Read con Include relacional
        var ventaDb = await context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == venta.Id);

        Assert.NotNull(ventaDb);
        Assert.Equal(cliente.Id, ventaDb.Cliente.Id);
        Assert.Single(ventaDb.Detalles);

        var detalleDb = ventaDb.Detalles.First();
        Assert.Equal(2, detalleDb.Cantidad);
        Assert.Equal(15000.00m, detalleDb.PrecioUnitario);
        Assert.Equal(30000.00m, detalleDb.Subtotal);
        Assert.Equal(producto.Nombre, detalleDb.Producto.Nombre);

        // 4. Cascade Delete: Eliminar venta debe eliminar detalles automáticamente
        context.Ventas.Remove(ventaDb);
        await context.SaveChangesAsync();

        var detalleHuerfano = await context.Detalles.FirstOrDefaultAsync(d => d.Id == detalle.Id);
        Assert.Null(detalleHuerfano); // Fue eliminado en cascada

        // Cliente y producto deben seguir existiendo
        var clientePersiste = await context.Clientes.FirstOrDefaultAsync(c => c.Id == cliente.Id);
        var productoPersiste = await context.Productos.FirstOrDefaultAsync(p => p.Id == producto.Id);
        Assert.NotNull(clientePersiste);
        Assert.NotNull(productoPersiste);

        // Limpieza
        context.Clientes.Remove(clientePersiste);
        context.Productos.Remove(productoPersiste);
        await context.SaveChangesAsync();
    }

    [Fact]
    public void DbContextModel_ConstraintsAndKeys_ShouldBeConfigured()
    {
        using var context = CreateContext();
        var model = context.Model;

        // Entidad Cliente
        var clienteEntity = model.FindEntityType(typeof(Cliente));
        Assert.NotNull(clienteEntity);
        Assert.Equal("clientes", clienteEntity.GetTableName());
        var clientePk = clienteEntity.FindPrimaryKey();
        Assert.NotNull(clientePk);
        Assert.Single(clientePk.Properties);
        Assert.Equal("Id", clientePk.Properties[0].Name);

        // Entidad Producto
        var productoEntity = model.FindEntityType(typeof(Producto));
        Assert.NotNull(productoEntity);
        Assert.Equal("productos", productoEntity.GetTableName());

        // Entidad Venta
        var ventaEntity = model.FindEntityType(typeof(Venta));
        Assert.NotNull(ventaEntity);
        Assert.Equal("ventas", ventaEntity.GetTableName());

        // Entidad Detalle
        var detalleEntity = model.FindEntityType(typeof(Detalle));
        Assert.NotNull(detalleEntity);
        Assert.Equal("detalles", detalleEntity.GetTableName());

        // Claves Foráneas de Detalle
        var fkDetalleVenta = detalleEntity.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType == ventaEntity);
        Assert.NotNull(fkDetalleVenta);

        var fkDetalleProducto = detalleEntity.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType == productoEntity);
        Assert.NotNull(fkDetalleProducto);
    }

    [Fact]
    public void Application_AddApplication_ShouldRegisterSuccessfully()
    {
        var services = new ServiceCollection();
        var result = services.AddApplication();

        Assert.NotNull(result);
        Assert.Same(services, result);
    }
}
