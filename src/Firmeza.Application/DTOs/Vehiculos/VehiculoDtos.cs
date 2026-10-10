using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Vehiculos;

public record VehiculoDto(
    Guid Id,
    string Placa,
    string Marca,
    string Modelo,
    int Anio,
    decimal CapacidadCargaKg,
    string Estado,
    bool Activo
);

public record CreateVehiculoDto(
    [Required(ErrorMessage = "La placa es obligatoria")]
    string Placa,

    [Required(ErrorMessage = "La marca es obligatoria")]
    string Marca,

    [Required(ErrorMessage = "El modelo es obligatorio")]
    string Modelo,

    [Range(1990, 2050, ErrorMessage = "Año fuera del rango permitido")]
    int Anio,

    [Range(0, double.MaxValue, ErrorMessage = "La capacidad no puede ser negativa")]
    decimal CapacidadCargaKg,

    string Estado = "Disponible"
);

public record UpdateVehiculoDto(
    [Required(ErrorMessage = "La placa es obligatoria")]
    string Placa,

    [Required(ErrorMessage = "La marca es obligatoria")]
    string Marca,

    [Required(ErrorMessage = "El modelo es obligatorio")]
    string Modelo,

    int Anio,
    decimal CapacidadCargaKg,
    string Estado,
    bool Activo
);
