using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TestIdentityCore.Api.Dto;
using TestIdentityCore.Domain;
using TestIdentityCore.Repository;

namespace TestIdentityCore.Api;

[Route("api/users")]
[ApiController]
public class UserController(
    IUserRepository userRepository,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admins can create users
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto model)
    {
        var existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return BadRequest("Email is already in use.");
        }

        var newUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await userManager.CreateAsync(newUser, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Assign roles if provided
        if (model.Roles.Count != 0)
        {
            await userManager.AddToRolesAsync(newUser, model.Roles);
        }

        return Ok(new UserReadDto
        {
            Id = newUser.Id.ToString(),
            FullName = newUser.FullName,
            Email = newUser.Email,
            Roles = model.Roles ?? []
        });
    }
    
    [HttpGet("me")]
    [Authorize] // Requires authentication
    public async Task<IActionResult> GetCurrentUser()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized("User not found.");
        }

        var roles = await userManager.GetRolesAsync(currentUser);

        return Ok(new UserReadDto()
        {
            Id = currentUser.Id.ToString(),
            FullName = currentUser.FullName,
            Email = currentUser.Email,
            Roles = roles.ToList()
        });
    }

    
    [HttpGet]
    [Authorize(Roles = "Admin")] // Only Admins can list users
    public async Task<IActionResult> GetUsers()
    {
        var users = await userRepository.GetAllAsync();

        // Convert to DTO before returning
        var userDtos = await Task.WhenAll(users.Select(async user => new UserReadDto
        {
            Id = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            Roles = await userManager.GetRolesAsync(user)
        }).ToList());

        return Ok(userDtos);
    }


    [HttpPut("{id}")]
    [Authorize] // Any authenticated user can access this endpoint
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto model)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized("User not found.");
        }

        // Check if the user is an admin or updating their own profile
        bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
        bool isEditingSelf = currentUser.Id.ToString() == id;

        if (!isAdmin && !isEditingSelf)
        {
            return Forbid("You can only edit your own profile.");
        }

        var userToUpdate = await userManager.FindByIdAsync(id);
        if (userToUpdate == null)
        {
            return NotFound("User not found.");
        }

        // Apply updates
        userToUpdate.FullName = model.FullName ?? userToUpdate.FullName;
        await userRepository.UpdateAsync(userToUpdate);
        return Ok(new UserReadDto
        {
            Id = userToUpdate.Id.ToString(),
            FullName = userToUpdate.FullName,
            Email = userToUpdate.Email,
        });
    }
    
    [HttpDelete("{id}")]
    [Authorize] // Authenticated users can access
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized("User not found.");
        }

        var userToDelete = await userManager.FindByIdAsync(id);
        if (userToDelete == null)
        {
            return NotFound("User not found.");
        }

        // Check if current user is an admin or deleting themselves
        var isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
        var isDeletingSelf = currentUser.Id.ToString() == id;

        if (!isAdmin && !isDeletingSelf)
        {
            return Forbid("You can only delete your own account.");
        }

        var result = await userManager.DeleteAsync(userToDelete);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok($"User {userToDelete.Email} deleted successfully.");
    }
}