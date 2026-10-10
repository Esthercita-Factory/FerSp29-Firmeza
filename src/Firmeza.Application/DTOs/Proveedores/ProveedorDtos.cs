using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Proveedores;

public record ProveedorDto(
    Guid Id,
    string RucOIdentificacion,
    string RazonSocial,
    string ContactoNombre,
    string Telefono,
    string Email,
    string Direccion,
    bool Activo,
    DateTime FechaRegistro
);

public record CreateProveedorDto(
    [Required(ErrorMessage = "El RUC o Identificación es obligatorio")]
    string RucOIdentificacion,

    [Required(ErrorMessage = "La Razón Social es obligatoria")]
    string RazonSocial,

    string ContactoNombre,
    string Telefono,

    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    string Email,

    string Direccion
);

public record UpdateProveedorDto(
    [Required(ErrorMessage = "El RUC o Identificación es obligatorio")]
    string RucOIdentificacion,

    [Required(ErrorMessage = "La Razón Social es obligatoria")]
    string RazonSocial,

    string ContactoNombre,
    string Telefono,

    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    string Email,

    string Direccion,
    bool Activo
);
