using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Empleados;

public record EmpleadoDto(
    string Id,
    string Email,
    string UserName,
    string? PhoneNumber,
    string Rol,
    bool Activo,
    DateTimeOffset? LockoutEnd
);

public record CreateEmpleadoAdminDto(
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres")]
    string Password,

    string? UserName,
    string? PhoneNumber,

    [Required(ErrorMessage = "El rol es obligatorio (Admin, Trabajador, Empleado)")]
    string Rol
);

public record UpdateEmpleadoAdminDto(
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    string Email,

    string? PhoneNumber,

    [Required(ErrorMessage = "El rol es obligatorio")]
    string Rol,

    bool Activo
);

public record CambiarEstadoEmpleadoDto(bool Activo);
