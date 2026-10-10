using Firmeza.Application.DTOs.Proveedores;

namespace Firmeza.Application.Interfaces;

public interface IProveedorService
{
    Task<IEnumerable<ProveedorDto>> GetAllAsync();
    Task<ProveedorDto?> GetByIdAsync(Guid id);
    Task<ProveedorDto> CreateAsync(CreateProveedorDto dto);
    Task<ProveedorDto?> UpdateAsync(Guid id, UpdateProveedorDto dto);
    Task<bool> DeleteAsync(Guid id);
}
