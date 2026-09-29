using System.Text.Json.Serialization;
using Firmeza.API.Extensions;
using Firmeza.Application;
using Firmeza.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Inyección de dependencias de capas de la solución
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Controladores con serialización JSON evitando ciclos relacionales
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 3. Seguridad: ASP.NET Core Identity y autenticación JWT
builder.Services.AddIdentityAndJwt(builder.Configuration);

// 4. Documentación interactiva de la API con Swagger y soporte Bearer JWT
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// 5. Configuración de interfaces de documentación interactiva (Swagger UI y Scalar)
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Firmeza API Reference")
               .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
    });
}

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();

// Middlewares de seguridad (Autenticación y Autorización)
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de Controladores RESTful
app.MapControllers();

app.Run();