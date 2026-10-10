using Firmeza.Application.DTOs.Vehiculos;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculoService _service;

    public VehiculosController(IVehiculoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehiculoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var vehiculo = await _service.GetByIdAsync(id);
        return vehiculo == null ? NotFound(new { message = $"Vehículo con ID '{id}' no encontrado." }) : Ok(vehiculo);
    }

    [HttpGet("estado/{estado}")]
    [ProducesResponseType(typeof(IEnumerable<VehiculoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEstado(string estado) => Ok(await _service.GetByEstadoAsync(estado));

    [HttpPost]
    [ProducesResponseType(typeof(VehiculoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVehiculoDto dto)
    {
        var nuevo = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, nuevo);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VehiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehiculoDto dto)
    {
        var actualizado = await _service.UpdateAsync(id, dto);
        return actualizado == null ? NotFound(new { message = $"Vehículo con ID '{id}' no encontrado." }) : Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminado = await _service.DeleteAsync(id);
        return eliminado ? NoContent() : NotFound(new { message = $"Vehículo con ID '{id}' no encontrado." });
    }
}
