using Application.Common;
using Application.DTOs.User;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Manager")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result);
        
        if(!_currentUserService.IsAdmin() && result.Data!.BranchId != _currentUserService.BranchId)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("by-username")]
    public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
    {
        var result = await _userService.GetUserByUsernameAsync(username);

        if (!result.IsSuccess)
            return NotFound(result);
        
        if(!_currentUserService.IsAdmin() && result.Data!.BranchId != _currentUserService.BranchId)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("by-email")]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        var result = await _userService.GetUserByEmailAsync(email);

        if (!result.IsSuccess)
            return NotFound(result);
        
        if(!_currentUserService.IsAdmin() && result.Data!.BranchId != _currentUserService.BranchId)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("by-branch/{branchId:guid}")]
    public async Task<IActionResult> GetUserByBranch(Guid branchId)
    {
        if(!_currentUserService.IsAdmin() && branchId != _currentUserService.BranchId)
            return Forbid();

        var result = await _userService.GetUsersByBranchIdAsync(branchId);

        if (!result.IsSuccess)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("by-role/{roleId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsersByRole(Guid roleId)
    {
        var result = await _userService.GetUsersByRoleIdAsync(roleId);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if(!_currentUserService.IsAdmin() && user.Data!.BranchId != _currentUserService.BranchId)
            return Forbid();

        var result = await _userService.UpdateUserAsync(id, updateUserDto);
        if (!result.IsSuccess)
            return BadRequest(result);


        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}