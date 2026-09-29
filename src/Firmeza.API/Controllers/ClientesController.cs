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
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista completa de clientes registrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Cliente>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
    {
        var clientes = await _context.Clientes.AsNoTracking().ToListAsync();
        return Ok(clientes);
    }

    /// <summary>
    /// Obtiene los detalles de un cliente por su identificador único (UUID).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Cliente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Cliente>> GetById(Guid id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null)
        {
            return NotFound(new { message = $"Cliente con ID '{id}' no encontrado." });
        }

        return Ok(cliente);
    }

    /// <summary>
    /// Registra un nuevo cliente en el sistema.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Cliente), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Cliente>> Create([FromBody] Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre) || string.IsNullOrWhiteSpace(cliente.Correo))
        {
            return BadRequest(new { message = "El nombre y el correo electrónico son campos requeridos." });
        }

        // Verificar si el correo ya está registrado
        var emailExiste = await _context.Clientes.AnyAsync(c => c.Correo.ToLower() == cliente.Correo.ToLower());
        if (emailExiste)
        {
            return BadRequest(new { message = $"Ya existe un cliente registrado con el correo '{cliente.Correo}'." });
        }

        if (cliente.Id == Guid.Empty)
        {
            cliente.Id = Guid.NewGuid();
        }

        if (cliente.FechaRegistro == default)
        {
            cliente.FechaRegistro = DateTime.UtcNow;
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    /// <summary>
    /// Actualiza la información de un cliente existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Cliente cliente)
    {
        if (cliente.Id != Guid.Empty && id != cliente.Id)
        {
            return BadRequest(new { message = "El identificador de la ruta no coincide con el identificador del cuerpo de la petición." });
        }

        var existing = await _context.Clientes.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { message = $"Cliente con ID '{id}' no encontrado." });
        }

        // Verificar unicidad de correo si cambió
        if (!string.Equals(existing.Correo, cliente.Correo, StringComparison.OrdinalIgnoreCase))
        {
            var emailEnUso = await _context.Clientes.AnyAsync(c => c.Id != id && c.Correo.ToLower() == cliente.Correo.ToLower());
            if (emailEnUso)
            {
                return BadRequest(new { message = $"El correo '{cliente.Correo}' ya se encuentra en uso por otro cliente." });
            }
        }

        existing.Nombre = cliente.Nombre;
        existing.Correo = cliente.Correo;
        existing.Telefono = cliente.Telefono;
        existing.Direccion = cliente.Direccion;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina un cliente por su identificador único.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound(new { message = $"Cliente con ID '{id}' no encontrado." });
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}