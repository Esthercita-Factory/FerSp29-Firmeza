using Firmeza.Application.DTOs.Empleados;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Firmeza.Infrastructure.Services;

public class AdminEmpleadoService : IAdminEmpleadoService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminEmpleadoService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    public async Task<(bool Success, string Message, EmpleadoDto? Empleado)> CreateAsync(CreateEmpleadoAdminDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return (false, "Ya existe un usuario con este correo electrónico.", null);

        await EnsureRoleExistsAsync(dto.Rol);

        var user = new IdentityUser
        {
            UserName = string.IsNullOrWhiteSpace(dto.UserName) ? dto.Email : dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Error al crear empleado: {errors}", null);
        }

        await _userManager.AddToRoleAsync(user, dto.Rol);

        var responseDto = new EmpleadoDto(
            user.Id,
            user.Email,
            user.UserName,
            user.PhoneNumber,
            dto.Rol,
            true,
            null
        );

        return (true, "Empleado creado exitosamente", responseDto);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(string id, UpdateEmpleadoAdminDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, "Empleado no encontrado.");

        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        if (dto.Activo)
        {
            user.LockoutEnd = null;
        }
        else
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return (false, $"Error al actualizar datos: {errors}");
        }

        await EnsureRoleExistsAsync(dto.Rol);
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, dto.Rol);

        return (true, "Empleado actualizado exitosamente.");
    }

    public async Task<(bool Success, string Message)> CambiarEstadoAsync(string id, bool activo)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, "Empleado no encontrado.");

        user.LockoutEnd = activo ? null : DateTimeOffset.UtcNow.AddYears(100);
        user.LockoutEnabled = true;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded 
            ? (true, $"Estado del empleado cambiado a {(activo ? "Activo" : "Inactivo")}.")
            : (false, "No se pudo cambiar el estado del empleado.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, "Empleado no encontrado.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? (true, "Empleado eliminado correctamente.")
            : (false, "No se pudo eliminar el empleado.");
    }
}
