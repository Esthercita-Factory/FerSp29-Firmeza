using System.Collections.Concurrent;
using Firmeza.Application.DTOs.Proveedores;
using Firmeza.Application.Interfaces;

namespace Firmeza.Infrastructure.Services;

public class ProveedorService : IProveedorService
{
    private static readonly ConcurrentDictionary<Guid, ProveedorDto> _proveedores = new();

    public Task<IEnumerable<ProveedorDto>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<ProveedorDto>>(_proveedores.Values.OrderBy(p => p.RazonSocial).ToList());
    }

    public Task<ProveedorDto?> GetByIdAsync(Guid id)
    {
        _proveedores.TryGetValue(id, out var proveedor);
        return Task.FromResult(proveedor);
    }

    public Task<ProveedorDto> CreateAsync(CreateProveedorDto dto)
    {
        var nuevo = new ProveedorDto(
            Id: Guid.NewGuid(),
            RucOIdentificacion: dto.RucOIdentificacion,
            RazonSocial: dto.RazonSocial,
            ContactoNombre: dto.ContactoNombre,
            Telefono: dto.Telefono,
            Email: dto.Email,
            Direccion: dto.Direccion,
            Activo: true,
            FechaRegistro: DateTime.UtcNow
        );

        _proveedores[nuevo.Id] = nuevo;
        return Task.FromResult(nuevo);
    }

    public Task<ProveedorDto?> UpdateAsync(Guid id, UpdateProveedorDto dto)
    {
        if (!_proveedores.TryGetValue(id, out var existente))
            return Task.FromResult<ProveedorDto?>(null);

        var actualizado = existente with
        {
            RucOIdentificacion = dto.RucOIdentificacion,
            RazonSocial = dto.RazonSocial,
            ContactoNombre = dto.ContactoNombre,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            Activo = dto.Activo
        };

        _proveedores[id] = actualizado;
        return Task.FromResult<ProveedorDto?>(actualizado);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_proveedores.TryRemove(id, out _));
    }
}
