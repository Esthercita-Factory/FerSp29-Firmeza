using System.Text.Json.Serialization;
using Firmeza.Application;
using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Inyección de dependencias de las capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configuración de serialización JSON para evitar ciclos en entidades relacionales
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Configuración de OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHttpsRedirection();

// ==========================================
// 1. Catálogo de Productos
// ==========================================
var productosGroup = app.MapGroup("/productos").WithTags("Productos");

productosGroup.MapGet("/", async (ApplicationDbContext db) =>
{
    var productos = await db.Productos.AsNoTracking().ToListAsync();
    return Results.Ok(productos);
})
.WithName("GetProductos")
.WithSummary("Obtener catálogo de productos");

productosGroup.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var producto = await db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    return producto is not null ? Results.Ok(producto) : Results.NotFound();
})
.WithName("GetProductoById")
.WithSummary("Obtener producto por ID");

// ==========================================
// 2. Registro de Clientes
// ==========================================
var clientesGroup = app.MapGroup("/clientes").WithTags("Clientes");

clientesGroup.MapGet("/", async (ApplicationDbContext db) =>
{
    var clientes = await db.Clientes.AsNoTracking().ToListAsync();
    return Results.Ok(clientes);
})
.WithName("GetClientes")
.WithSummary("Obtener lista de clientes");

clientesGroup.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    return cliente is not null ? Results.Ok(cliente) : Results.NotFound();
})
.WithName("GetClienteById")
.WithSummary("Obtener cliente por ID");

// ==========================================
// 3. Registro de Ventas
// ==========================================
var ventasGroup = app.MapGroup("/ventas").WithTags("Ventas");

ventasGroup.MapGet("/", async (ApplicationDbContext db) =>
{
    var ventas = await db.Ventas.AsNoTracking().ToListAsync();
    return Results.Ok(ventas);
})
.WithName("GetVentas")
.WithSummary("Obtener lista de ventas");

ventasGroup.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var venta = await db.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
    return venta is not null ? Results.Ok(venta) : Results.NotFound();
})
.WithName("GetVentaById")
.WithSummary("Obtener venta por ID");

// ==========================================
// 4. Detalles de Venta
// ==========================================
var detallesGroup = app.MapGroup("/detalles").WithTags("Detalles");

detallesGroup.MapGet("/", async (ApplicationDbContext db) =>
{
    var detalles = await db.Detalles.AsNoTracking().ToListAsync();
    return Results.Ok(detalles);
})
.WithName("GetDetalles")
.WithSummary("Obtener lista de detalles de venta");

detallesGroup.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var detalle = await db.Detalles.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
    return detalle is not null ? Results.Ok(detalle) : Results.NotFound();
})
.WithName("GetDetalleById")
.WithSummary("Obtener detalle por ID");

// ==========================================
// Compatibilidad retroactiva para prefijos /api
// ==========================================
app.MapGet("/api/productos", async (ApplicationDbContext db) => Results.Ok(await db.Productos.AsNoTracking().ToListAsync())).ExcludeFromDescription();
app.MapGet("/api/clientes", async (ApplicationDbContext db) => Results.Ok(await db.Clientes.AsNoTracking().ToListAsync())).ExcludeFromDescription();
app.MapGet("/api/ventas", async (ApplicationDbContext db) => Results.Ok(await db.Ventas.AsNoTracking().ToListAsync())).ExcludeFromDescription();
app.MapGet("/api/detalles", async (ApplicationDbContext db) => Results.Ok(await db.Detalles.AsNoTracking().ToListAsync())).ExcludeFromDescription();

app.Run();
