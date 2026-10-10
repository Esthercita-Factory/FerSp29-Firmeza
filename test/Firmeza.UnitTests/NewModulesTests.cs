using System.Reflection;
using System.Security.Claims;
using Firmeza.API.Controllers;
using Firmeza.Application.DTOs.Empleados;
using Firmeza.Application.DTOs.Inventarios;
using Firmeza.Application.DTOs.MovimientosPedidos;
using Firmeza.Application.DTOs.Proveedores;
using Firmeza.Application.DTOs.Reportes;
using Firmeza.Application.DTOs.Vehiculos;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Firmeza.UnitTests;

public class NewModulesTests
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
    public void Security_AuthorizeAndRoleAttributes_AreProperlyConfigured()
    {
        // 1. Verificar [Authorize] en controladores generales
        var proveedoresAuth = typeof(ProveedoresController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(proveedoresAuth);
        Assert.Null(proveedoresAuth.Roles);

        var empleadosAuth = typeof(EmpleadosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(empleadosAuth);
        Assert.Null(empleadosAuth.Roles);

        var vehiculosAuth = typeof(VehiculosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(vehiculosAuth);
        Assert.Null(vehiculosAuth.Roles);

        var inventariosAuth = typeof(InventariosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(inventariosAuth);
        Assert.Null(inventariosAuth.Roles);

        var movimientosAuth = typeof(MovimientosPedidosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(movimientosAuth);
        Assert.Null(movimientosAuth.Roles);

        // 2. Verificar [Authorize(Roles = "Admin")] en controladores administrativos y de reportes
        var adminEmpleadosAuth = typeof(AdminEmpleadosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(adminEmpleadosAuth);
        Assert.Equal("Admin", adminEmpleadosAuth.Roles);

        var reportesAuth = typeof(ReportesController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(reportesAuth);
        Assert.Equal("Admin", reportesAuth.Roles);
    }

    [Fact]
    public async Task Proveedores_CRUD_ShouldWorkCorrectly()
    {
        IProveedorService service = new ProveedorService();
        var controller = new ProveedoresController(service);

        var testRuc = $"RUC-{Guid.NewGuid():N}"[..10];
        var createDto = new CreateProveedorDto(
            RucOIdentificacion: testRuc,
            RazonSocial: "Ferretera Industrial Andina S.A.S.",
            ContactoNombre: "Carlos Gómez",
            Telefono: "3001234567",
            Email: "ventas@andinaferretera.com",
            Direccion: "Calle 100 # 20-30"
        );

        // 1. Create (POST)
        var createResult = await controller.Create(createDto);
        var createdAction = Assert.IsType<CreatedAtActionResult>(createResult);
        var creado = Assert.IsType<ProveedorDto>(createdAction.Value);
        Assert.NotEqual(Guid.Empty, creado.Id);
        Assert.Equal(testRuc, creado.RucOIdentificacion);
        Assert.True(creado.Activo);

        // 2. GetById (GET)
        var getResult = await controller.GetById(creado.Id);
        var okGet = Assert.IsType<OkObjectResult>(getResult);
        var obtenido = Assert.IsType<ProveedorDto>(okGet.Value);
        Assert.Equal(creado.Id, obtenido.Id);

        // 3. GetAll (GET)
        var allResult = await controller.GetAll();
        var okAll = Assert.IsType<OkObjectResult>(allResult);
        var lista = Assert.IsAssignableFrom<IEnumerable<ProveedorDto>>(okAll.Value);
        Assert.Contains(lista, p => p.Id == creado.Id);

        // 4. Update (PUT)
        var updateDto = new UpdateProveedorDto(
            RucOIdentificacion: testRuc,
            RazonSocial: "Ferretera Industrial Andina Actualizada",
            ContactoNombre: "Carlos Gómez Jr.",
            Telefono: "3119876543",
            Email: "contacto@andinaferretera.com",
            Direccion: "Carrera 50 # 80-10",
            Activo: true
        );
        var updateResult = await controller.Update(creado.Id, updateDto);
        var okUpdate = Assert.IsType<OkObjectResult>(updateResult);
        var actualizado = Assert.IsType<ProveedorDto>(okUpdate.Value);
        Assert.Equal("Ferretera Industrial Andina Actualizada", actualizado.RazonSocial);
        Assert.Equal("3119876543", actualizado.Telefono);

        // 5. Delete (DELETE)
        var deleteResult = await controller.Delete(creado.Id);
        Assert.IsType<NoContentResult>(deleteResult);

        // 6. Verify Deleted (404)
        var notFoundResult = await controller.GetById(creado.Id);
        Assert.IsType<NotFoundObjectResult>(notFoundResult);
    }

    [Fact]
    public async Task Vehiculos_CRUD_And_Filter_ShouldWorkCorrectly()
    {
        IVehiculoService service = new VehiculoService();
        var controller = new VehiculosController(service);

        var testPlaca = $"PLK{Random.Shared.Next(100, 999)}";
        var createDto = new CreateVehiculoDto(
            Placa: testPlaca,
            Marca: "Chevrolet",
            Modelo: "NKR Camión",
            Anio: 2024,
            CapacidadCargaKg: 3500.50m,
            Estado: "Disponible"
        );

        // 1. Create (POST)
        var createResult = await controller.Create(createDto);
        var createdAction = Assert.IsType<CreatedAtActionResult>(createResult);
        var creado = Assert.IsType<VehiculoDto>(createdAction.Value);
        Assert.NotEqual(Guid.Empty, creado.Id);
        Assert.Equal(testPlaca, creado.Placa);
        Assert.Equal("Disponible", creado.Estado);

        // 2. GetById (GET)
        var getResult = await controller.GetById(creado.Id);
        var okGet = Assert.IsType<OkObjectResult>(getResult);
        var obtenido = Assert.IsType<VehiculoDto>(okGet.Value);
        Assert.Equal(creado.Id, obtenido.Id);

        // 3. Filter by Estado (GET)
        var filterResult = await controller.GetByEstado("Disponible");
        var okFilter = Assert.IsType<OkObjectResult>(filterResult);
        var lista = Assert.IsAssignableFrom<IEnumerable<VehiculoDto>>(okFilter.Value);
        Assert.Contains(lista, v => v.Id == creado.Id);

        // 4. Update (PUT)
        var updateDto = new UpdateVehiculoDto(
            Placa: testPlaca,
            Marca: "Chevrolet",
            Modelo: "NKR Camión Plus",
            Anio: 2024,
            CapacidadCargaKg: 4000.00m,
            Estado: "En Ruta",
            Activo: true
        );
        var updateResult = await controller.Update(creado.Id, updateDto);
        var okUpdate = Assert.IsType<OkObjectResult>(updateResult);
        var actualizado = Assert.IsType<VehiculoDto>(okUpdate.Value);
        Assert.Equal("En Ruta", actualizado.Estado);
        Assert.Equal(4000.00m, actualizado.CapacidadCargaKg);

        // 5. Delete (DELETE)
        var deleteResult = await controller.Delete(creado.Id);
        Assert.IsType<NoContentResult>(deleteResult);
    }

    [Fact]
    public async Task Inventario_Consultas_YAjustes_ShouldWorkCorrectly()
    {
        using var context = CreateContext();
        IInventarioService service = new InventarioService(context);
        var controller = new InventariosController(service);

        // Crear producto de prueba
        var prod = new Producto
        {
            Id = Guid.NewGuid(),
            Codigo = $"INV-{Guid.NewGuid():N}"[..12],
            Nombre = "Cemento Gris Estructural 50kg",
            Descripcion = "Cemento para construcción pesada",
            Precio = 28000m,
            Stock = 20,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        context.Productos.Add(prod);
        await context.SaveChangesAsync();

        // 1. Obtener Stock por Producto (GET)
        var stockResult = await controller.ObtenerStockPorProducto(prod.Id);
        var okStock = Assert.IsType<OkObjectResult>(stockResult);
        var stockDto = Assert.IsType<StockProductoDto>(okStock.Value);
        Assert.Equal(20, stockDto.StockActual);

        // 2. Ajustar Stock - Incrementar (+15)
        var ajusteInc = new AjusteStockDto(prod.Id, 15, "Incrementar", "Ingreso de lote nuevo");
        var incResult = await controller.AjustarStock(ajusteInc);
        Assert.IsType<OkObjectResult>(incResult);

        var verifyInc = await controller.ObtenerStockPorProducto(prod.Id);
        var okVerifyInc = Assert.IsType<OkObjectResult>(verifyInc);
        var stockAfterInc = Assert.IsType<StockProductoDto>(okVerifyInc.Value);
        Assert.Equal(35, stockAfterInc.StockActual);

        // 3. Ajustar Stock - Decrementar (-10)
        var ajusteDec = new AjusteStockDto(prod.Id, 10, "Decrementar", "Uso interno en obra");
        var decResult = await controller.AjustarStock(ajusteDec);
        Assert.IsType<OkObjectResult>(decResult);

        var verifyDec = await controller.ObtenerStockPorProducto(prod.Id);
        var okVerifyDec = Assert.IsType<OkObjectResult>(verifyDec);
        var stockAfterDec = Assert.IsType<StockProductoDto>(okVerifyDec.Value);
        Assert.Equal(25, stockAfterDec.StockActual);

        // 4. Ajustar Stock - Decrementar más de lo disponible (debe fallar con 400)
        var ajusteExcesivo = new AjusteStockDto(prod.Id, 9999, "Decrementar", "Intento inválido");
        var badDecResult = await controller.AjustarStock(ajusteExcesivo);
        Assert.IsType<BadRequestObjectResult>(badDecResult);

        // 5. Resumen de Inventario (GET)
        var resumenResult = await controller.ObtenerResumen();
        var okResumen = Assert.IsType<OkObjectResult>(resumenResult);
        var resumen = Assert.IsType<ResumenInventarioDto>(okResumen.Value);
        Assert.True(resumen.TotalProductos > 0);
        Assert.True(resumen.TotalStockUnidades > 0);

        // Limpieza
        context.Productos.Remove(prod);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task MovimientosYPedidos_TransactionFlow_ShouldSucceed()
    {
        using var context = CreateContext();
        IMovimientosPedidosService service = new MovimientosPedidosService(context);
        var controller = new MovimientosPedidosController(service);

        // Mock de usuario autenticado en el HttpContext del Controller
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "operador@firmeza.com")
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Crear cliente y producto para el pedido
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = "Constructora del Valle",
            Correo = $"cliente_pedido_{Guid.NewGuid():N}@valle.com",
            Telefono = "3201114455",
            Direccion = "Zona Industrial Lote 4"
        };
        var prod = new Producto
        {
            Id = Guid.NewGuid(),
            Codigo = $"MOV-{Guid.NewGuid():N}"[..12],
            Nombre = "Pintura Epóxica Gris Galón",
            Descripcion = "Pintura de alto tráfico",
            Precio = 85000m,
            Stock = 50,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        context.Clientes.Add(cliente);
        context.Productos.Add(prod);
        await context.SaveChangesAsync();

        // 1. Registrar Movimiento de Entrada (+20)
        var movEntrada = new RegistrarMovimientoDto(prod.Id, 20, "Entrada", "Recepción de fábrica", "FACT-10023");
        var resEntrada = await controller.RegistrarMovimiento(movEntrada);
        Assert.IsType<CreatedResult>(resEntrada);

        // Verificar que el stock subió a 70
        var prodDb = await context.Productos.FindAsync(prod.Id);
        Assert.NotNull(prodDb);
        Assert.Equal(70, prodDb.Stock);

        // 2. Crear Pedido (Venta de 5 unidades)
        var pedidoDto = new CrearPedidoDto(
            ClienteId: cliente.Id,
            Items: new List<ItemPedidoDto>
            {
                new ItemPedidoDto(prod.Id, 5)
            },
            Observaciones: "Entrega urgente en bodega"
        );

        var resPedido = await controller.CrearPedido(pedidoDto);
        var createdAction = Assert.IsType<CreatedAtActionResult>(resPedido);
        var pedidoRespuesta = Assert.IsType<PedidoRespuestaDto>(createdAction.Value);

        Assert.Equal(cliente.Id, pedidoRespuesta.ClienteId);
        Assert.Equal(425000m, pedidoRespuesta.Total); // 5 * 85,000 = 425,000
        Assert.Single(pedidoRespuesta.Detalles);

        // Verificar que el stock bajó a 65
        await context.Entry(prodDb).ReloadAsync();
        Assert.Equal(65, prodDb.Stock);

        // 3. Consultar Pedidos
        var pedidosResult = await controller.ObtenerPedidos();
        var okPedidos = Assert.IsType<OkObjectResult>(pedidosResult);
        var listaPedidos = Assert.IsAssignableFrom<IEnumerable<PedidoRespuestaDto>>(okPedidos.Value);
        Assert.Contains(listaPedidos, p => p.VentaId == pedidoRespuesta.VentaId);

        // Limpieza transaccional
        var ventaDb = await context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == pedidoRespuesta.VentaId);
        if (ventaDb != null)
        {
            context.Detalles.RemoveRange(ventaDb.Detalles);
            context.Ventas.Remove(ventaDb);
        }
        context.Productos.Remove(prod);
        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Reportes_AlertsAndMetrics_ShouldWorkCorrectly()
    {
        using var context = CreateContext();
        // Crear un producto con stock crítico (<= 5)
        var prodCritico = new Producto
        {
            Id = Guid.NewGuid(),
            Codigo = $"REP-{Guid.NewGuid():N}"[..12],
            Nombre = "Tornillo Drywall 1 pulgada",
            Descripcion = "Tornillo para yeso",
            Precio = 150m,
            Stock = 4,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        context.Productos.Add(prodCritico);
        await context.SaveChangesAsync();

        // Probar servicio de reportes para alertas de stock
        var reportesService = new ReportesService(context, null!);
        var alertas = await reportesService.ObtenerAlertasStockBajoAsync(10);
        Assert.Contains(alertas, a => a.ProductoId == prodCritico.Id);

        var topProductos = await reportesService.ObtenerTopProductosAsync(10);
        Assert.NotNull(topProductos);

        var ventasMes = await reportesService.ObtenerVentasPorMesAsync();
        Assert.NotNull(ventasMes);

        // Limpieza
        context.Productos.Remove(prodCritico);
        await context.SaveChangesAsync();
    }
}
