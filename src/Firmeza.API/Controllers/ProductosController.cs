using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el catálogo completo de productos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Producto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll()
    {
        var productos = await _context.Productos.AsNoTracking().ToListAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene los detalles de un producto por su identificador único (UUID).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Producto>> GetById(Guid id)
    {
        var producto = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null)
        {
            return NotFound(new { message = $"Producto con ID '{id}' no encontrado." });
        }

        return Ok(producto);
    }

    /// <summary>
    /// Registra un nuevo producto en el inventario.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Codigo) || string.IsNullOrWhiteSpace(producto.Nombre))
        {
            return BadRequest(new { message = "El código y el nombre del producto son campos obligatorios." });
        }

        if (producto.Precio < 0)
        {
            return BadRequest(new { message = "El precio del producto no puede ser negativo." });
        }

        if (producto.Stock < 0)
        {
            return BadRequest(new { message = "El stock del producto no puede ser negativo." });
        }

        // Verificar si el código ya existe
        var codigoExiste = await _context.Productos.AnyAsync(p => p.Codigo.ToUpper() == producto.Codigo.ToUpper());
        if (codigoExiste)
        {
            return BadRequest(new { message = $"Ya existe un producto con el código '{producto.Codigo}'." });
        }

        if (producto.Id == Guid.Empty)
        {
            producto.Id = Guid.NewGuid();
        }

        if (producto.FechaCreacion == default)
        {
            producto.FechaCreacion = DateTime.UtcNow;
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    /// <summary>
    /// Actualiza los datos de un producto existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Producto producto)
    {
        if (producto.Id != Guid.Empty && id != producto.Id)
        {
            return BadRequest(new { message = "El identificador de la ruta no coincide con el identificador del cuerpo de la petición." });
        }

        if (producto.Precio < 0)
        {
            return BadRequest(new { message = "El precio del producto no puede ser negativo." });
        }

        if (producto.Stock < 0)
        {
            return BadRequest(new { message = "El stock del producto no puede ser negativo." });
        }

        var existing = await _context.Productos.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { message = $"Producto con ID '{id}' no encontrado." });
        }

        // Verificar unicidad de código si cambió
        if (!string.Equals(existing.Codigo, producto.Codigo, StringComparison.OrdinalIgnoreCase))
        {
            var codigoEnUso = await _context.Productos.AnyAsync(p => p.Id != id && p.Codigo.ToUpper() == producto.Codigo.ToUpper());
            if (codigoEnUso)
            {
                return BadRequest(new { message = $"El código '{producto.Codigo}' ya se encuentra asignado a otro producto." });
            }
        }

        existing.Codigo = producto.Codigo;
        existing.Nombre = producto.Nombre;
        existing.Descripcion = producto.Descripcion;
        existing.Precio = producto.Precio;
        existing.Stock = producto.Stock;
        existing.Activo = producto.Activo;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina un producto del catálogo por su identificador único.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound(new { message = $"Producto con ID '{id}' no encontrado." });
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}