using Firmeza.Application.DTOs.Empleados;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoService _service;

    public EmpleadosController(IEmpleadoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var empleado = await _service.GetByIdAsync(id);
        return empleado == null ? NotFound(new { message = $"Empleado con ID '{id}' no encontrado." }) : Ok(empleado);
    }

    [HttpGet("rol/{rol}")]
    [ProducesResponseType(typeof(IEnumerable<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRol(string rol) => Ok(await _service.GetByRolAsync(rol));
}
