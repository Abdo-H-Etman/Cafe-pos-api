using Core.Application.DTOs.Branch;
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize(Roles = "Admin")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBranches(CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetAllBranchesAsync(cancellationToken);
        return Ok(branches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBranchById(Guid id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetBranchByIdAsync(id, cancellationToken);
        if (branch == null)
            return NotFound();
        return Ok(branch);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetBranchDetailsById(Guid id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetBranchDetailsByIdAsync(id, cancellationToken);
        if (branch == null)
            return NotFound();
        return Ok(branch);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBranch(CreateBranchDto createBranchDto, CancellationToken cancellationToken)
    {
        var branch = await _branchService.CreateBranchAsync(createBranchDto, cancellationToken);
        if (branch == null)
            return BadRequest();
        return CreatedAtAction(nameof(GetBranchById), new { id = branch.Data!.Id }, branch);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(Guid id, UpdateBranchDto updateBranchDto, CancellationToken cancellationToken)
    {
        var updatedBranch = await _branchService.UpdateBranchAsync(id, updateBranchDto, cancellationToken);
        if (updatedBranch == null)
            return NotFound();
        return Ok(updatedBranch);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(Guid id, CancellationToken cancellationToken)
    {
        var result = await _branchService.DeleteBranchAsync(id, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

}