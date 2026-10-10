using Firmeza.Application.DTOs.Empleados;

namespace Firmeza.Application.Interfaces;

public interface IAdminEmpleadoService
{
    Task<(bool Success, string Message, EmpleadoDto? Empleado)> CreateAsync(CreateEmpleadoAdminDto dto);
    Task<(bool Success, string Message)> UpdateAsync(string id, UpdateEmpleadoAdminDto dto);
    Task<(bool Success, string Message)> CambiarEstadoAsync(string id, bool activo);
    Task<(bool Success, string Message)> DeleteAsync(string id);
}
