using Firmeza.Application.DTOs.Empleados;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class EmpleadoService : IEmpleadoService
{
    private readonly UserManager<IdentityUser> _userManager;

    public EmpleadoService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<EmpleadoDto>> GetAllAsync()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();
        var list = new List<EmpleadoDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var rol = roles.FirstOrDefault() ?? "Sin Rol";
            var activo = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            list.Add(new EmpleadoDto(
                user.Id,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty,
                user.PhoneNumber,
                rol,
                activo,
                user.LockoutEnd
            ));
        }

        return list;
    }

    public async Task<EmpleadoDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var rol = roles.FirstOrDefault() ?? "Sin Rol";
        var activo = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new EmpleadoDto(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.PhoneNumber,
            rol,
            activo,
            user.LockoutEnd
        );
    }

    public async Task<IEnumerable<EmpleadoDto>> GetByRolAsync(string rol)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(rol);
        return usersInRole.Select(user => new EmpleadoDto(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.PhoneNumber,
            rol,
            user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
            user.LockoutEnd
        )).ToList();
    }
}
