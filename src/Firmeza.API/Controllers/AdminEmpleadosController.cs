using Firmeza.Application.DTOs.Empleados;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminEmpleadosController : ControllerBase
{
    private readonly IAdminEmpleadoService _service;

    public AdminEmpleadosController(IAdminEmpleadoService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmpleadoAdminDto dto)
    {
        var (success, message, empleado) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });

        return CreatedAtAction("GetById", "Empleados", new { id = empleado!.Id }, empleado);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEmpleadoAdminDto dto)
    {
        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPatch("{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarEstado(string id, [FromBody] CambiarEstadoEmpleadoDto dto)
    {
        var (success, message) = await _service.CambiarEstadoAsync(id, dto.Activo);
        if (!success) return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });

        return NoContent();
    }
}
