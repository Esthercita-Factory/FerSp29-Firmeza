using Firmeza.Application.DTOs.Proveedores;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _service;

    public ProveedoresController(IProveedorService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var proveedor = await _service.GetByIdAsync(id);
        return proveedor == null ? NotFound(new { message = $"Proveedor con ID '{id}' no encontrado." }) : Ok(proveedor);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProveedorDto dto)
    {
        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProveedorDto dto)
    {
        var actualizado = await _service.UpdateAsync(id, dto);
        return actualizado == null ? NotFound(new { message = $"Proveedor con ID '{id}' no encontrado." }) : Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { message = $"Proveedor con ID '{id}' no encontrado." });
    }
}
