using Firmeza.Application.DTOs.Vehiculos;

namespace Firmeza.Application.Interfaces;

public interface IVehiculoService
{
    Task<IEnumerable<VehiculoDto>> GetAllAsync();
    Task<VehiculoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<VehiculoDto>> GetByEstadoAsync(string estado);
    Task<VehiculoDto> CreateAsync(CreateVehiculoDto dto);
    Task<VehiculoDto?> UpdateAsync(Guid id, UpdateVehiculoDto dto);
    Task<bool> DeleteAsync(Guid id);
}
