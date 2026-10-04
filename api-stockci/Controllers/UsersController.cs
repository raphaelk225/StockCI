using api_stockci.Dtos;
using api_stockci.Models;
using api_stockci.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_stockci.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Role = dto.Role
        };
        await _userService.CreateAsync(user, dto.Password);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, UpdateUserDto dto)
    {
        var existing = await _userService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        existing.Username = dto.Username;
        existing.Email = dto.Email;
        existing.Role = dto.Role;

        await _userService.UpdateAsync(id, existing);
        return NoContent();
    }

    [HttpPut("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(string id, ChangePasswordDto dto)
    {
        var success = await _userService.ChangePasswordAsync(id, dto.OldPassword, dto.NewPassword);
        if (!success) return BadRequest(new { message = "Ancien mot de passe incorrect ou utilisateur introuvable." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var existing = await _userService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        await _userService.DeleteAsync(id);
        return NoContent();
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role,
        CreatedAt = user.CreatedAt
    };
}
