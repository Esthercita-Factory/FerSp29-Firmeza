using System.Reflection;
using Firmeza.API.Controllers;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Firmeza.UnitTests;

public class ControllersTests
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
    public void Controllers_MustHaveAuthorizeAttribute()
    {
        // Verifica que los controladores administrativos requieran autorización
        var clientesAttr = typeof(ClientesController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(clientesAttr);

        var productosAttr = typeof(ProductosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(productosAttr);
    }

    [Fact]
    public async Task ClientesController_CRUD_ShouldSucceed()
    {
        using var context = CreateContext();
        var controller = new ClientesController(context);

        var testEmail = $"cliente_ctrl_{Guid.NewGuid():N}@firmeza.com";
        var cliente = new Cliente
        {
            Nombre = "Cliente Controller Test",
            Correo = testEmail,
            Telefono = "3159998877",
            Direccion = "Carrera 15 # 45-67"
        };

        // 1. Create (POST)
        var createResult = await controller.Create(cliente);
        var createdAction = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdCliente = Assert.IsType<Cliente>(createdAction.Value);
        Assert.NotEqual(Guid.Empty, createdCliente.Id);
        Assert.Equal(testEmail, createdCliente.Correo);

        // 2. GetById (GET)
        var getResult = await controller.GetById(createdCliente.Id);
        var okGetResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var retrieved = Assert.IsType<Cliente>(okGetResult.Value);
        Assert.Equal("Cliente Controller Test", retrieved.Nombre);

        // 3. Update (PUT)
        retrieved.Telefono = "3001112233";
        retrieved.Direccion = "Avenida 68 # 100-20";
        var updateResult = await controller.Update(retrieved.Id, retrieved);
        Assert.IsType<NoContentResult>(updateResult);

        // Verificar cambio
        var verifyResult = await controller.GetById(retrieved.Id);
        var okVerify = Assert.IsType<OkObjectResult>(verifyResult.Result);
        var updatedCliente = Assert.IsType<Cliente>(okVerify.Value);
        Assert.Equal("3001112233", updatedCliente.Telefono);
        Assert.Equal("Avenida 68 # 100-20", updatedCliente.Direccion);

        // 4. Update con ID dispar (PUT debe retornar 400)
        var badUpdate = await controller.Update(Guid.NewGuid(), retrieved);
        Assert.IsType<BadRequestObjectResult>(badUpdate);

        // 5. Delete (DELETE)
        var deleteResult = await controller.Delete(retrieved.Id);
        Assert.IsType<NoContentResult>(deleteResult);

        // Verificar que ya no existe (404)
        var notFoundResult = await controller.GetById(retrieved.Id);
        Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
    }

    [Fact]
    public async Task ProductosController_CRUD_ShouldSucceed()
    {
        using var context = CreateContext();
        var controller = new ProductosController(context);

        var testCode = $"PRD-{Guid.NewGuid():N}"[..12];
        var producto = new Producto
        {
            Codigo = testCode,
            Nombre = "Martillo Especial Test",
            Descripcion = "Martillo de acero reforzado",
            Precio = 35000.50m,
            Stock = 50,
            Activo = true
        };

        // 1. Create (POST)
        var createResult = await controller.Create(producto);
        var createdAction = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdProducto = Assert.IsType<Producto>(createdAction.Value);
        Assert.NotEqual(Guid.Empty, createdProducto.Id);
        Assert.Equal(testCode, createdProducto.Codigo);

        // 2. GetById (GET)
        var getResult = await controller.GetById(createdProducto.Id);
        var okGetResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var retrieved = Assert.IsType<Producto>(okGetResult.Value);
        Assert.Equal("Martillo Especial Test", retrieved.Nombre);

        // 3. Update (PUT)
        retrieved.Precio = 38000.00m;
        retrieved.Stock = 45;
        var updateResult = await controller.Update(retrieved.Id, retrieved);
        Assert.IsType<NoContentResult>(updateResult);

        // Verificar cambio
        var verifyResult = await controller.GetById(retrieved.Id);
        var okVerify = Assert.IsType<OkObjectResult>(verifyResult.Result);
        var updatedProducto = Assert.IsType<Producto>(okVerify.Value);
        Assert.Equal(38000.00m, updatedProducto.Precio);
        Assert.Equal(45, updatedProducto.Stock);

        // 4. Update con precio negativo (debe retornar 400)
        updatedProducto.Precio = -10;
        var badUpdate = await controller.Update(updatedProducto.Id, updatedProducto);
        Assert.IsType<BadRequestObjectResult>(badUpdate);

        // 5. Delete (DELETE)
        var deleteResult = await controller.Delete(retrieved.Id);
        Assert.IsType<NoContentResult>(deleteResult);

        // Verificar que ya no existe (404)
        var notFoundResult = await controller.GetById(retrieved.Id);
        Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
    }
}
