using AutoPartsSystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController(IRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] string? group)
    {
        var parts = await repo.SearchPartsAsync(q, group);
        return Ok(parts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var part = await repo.GetPartByIdAsync(id);
        if (part == null) return NotFound();
        return Ok(part);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        var parts = await repo.GetAllPartsAsync();
        var groups = parts.Select(p => p.GroupName).Distinct().OrderBy(g => g).ToList();
        return Ok(groups);
    }
}
