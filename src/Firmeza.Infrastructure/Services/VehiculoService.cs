using System.Collections.Concurrent;
using Firmeza.Application.DTOs.Vehiculos;
using Firmeza.Application.Interfaces;

namespace Firmeza.Infrastructure.Services;

public class VehiculoService : IVehiculoService
{
    private static readonly ConcurrentDictionary<Guid, VehiculoDto> _vehiculos = new();

    public Task<IEnumerable<VehiculoDto>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<VehiculoDto>>(_vehiculos.Values.OrderBy(v => v.Placa).ToList());
    }

    public Task<VehiculoDto?> GetByIdAsync(Guid id)
    {
        _vehiculos.TryGetValue(id, out var vehiculo);
        return Task.FromResult(vehiculo);
    }

    public Task<IEnumerable<VehiculoDto>> GetByEstadoAsync(string estado)
    {
        var filtrados = _vehiculos.Values
            .Where(v => string.Equals(v.Estado, estado, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IEnumerable<VehiculoDto>>(filtrados);
    }

    public Task<VehiculoDto> CreateAsync(CreateVehiculoDto dto)
    {
        var nuevo = new VehiculoDto(
            Id: Guid.NewGuid(),
            Placa: dto.Placa.ToUpper(),
            Marca: dto.Marca,
            Modelo: dto.Modelo,
            Anio: dto.Anio,
            CapacidadCargaKg: dto.CapacidadCargaKg,
            Estado: dto.Estado,
            Activo: true
        );

        _vehiculos[nuevo.Id] = nuevo;
        return Task.FromResult(nuevo);
    }

    public Task<VehiculoDto?> UpdateAsync(Guid id, UpdateVehiculoDto dto)
    {
        if (!_vehiculos.TryGetValue(id, out var existente))
            return Task.FromResult<VehiculoDto?>(null);

        var actualizado = existente with
        {
            Placa = dto.Placa.ToUpper(),
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Anio = dto.Anio,
            CapacidadCargaKg = dto.CapacidadCargaKg,
            Estado = dto.Estado,
            Activo = dto.Activo
        };

        _vehiculos[id] = actualizado;
        return Task.FromResult<VehiculoDto?>(actualizado);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_vehiculos.TryRemove(id, out _));
    }
}
