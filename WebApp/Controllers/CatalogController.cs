using AutoPartsSystem.Data;
using AutoPartsSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly IRepository _repo;

    public CatalogController(IRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? q, [FromQuery] string? group)
    {
        var parts = _repo.SearchParts(q, group);
        return Ok(parts);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var part = _repo.GetPartById(id);
        if (part == null) return NotFound();
        return Ok(part);
    }

    [HttpGet("groups")]
    public IActionResult GetGroups()
    {
        var parts = _repo.GetAllParts();
        var groups = parts.Select(p => p.GroupName).Distinct().OrderBy(g => g).ToList();
        return Ok(groups);
    }
}
