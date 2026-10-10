using Firmeza.Application.DTOs.Empleados;

namespace Firmeza.Application.Interfaces;

public interface IEmpleadoService
{
    Task<IEnumerable<EmpleadoDto>> GetAllAsync();
    Task<EmpleadoDto?> GetByIdAsync(string id);
    Task<IEnumerable<EmpleadoDto>> GetByRolAsync(string rol);
}
