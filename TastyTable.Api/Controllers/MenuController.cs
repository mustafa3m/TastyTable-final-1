using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TastyTable.Core.DTOs;
using TastyTable.Core.Interfaces;

namespace TastyTable.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _service;
    public MenuController(IMenuService service) => _service = service;

    [AllowAnonymous] // public
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [Authorize(Roles = "Admin")] // secured
    [HttpPost]
    public async Task<IActionResult> Create(MenuItemCreateDto dto)
    {
        var item = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [AllowAnonymous] // public
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "Admin")] // secured
    [HttpPatch("{id:int}/availability")]
    public async Task<IActionResult> SetAvailability(int id, [FromQuery] bool isAvailable = true)
    {
        var item = await _service.UpdateAvailabilityAsync(id, isAvailable);
        return item is null ? NotFound() : Ok(item);
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? Ok(new { message = "Menu deleted successfully" })
                  : NotFound(new { message = "Menu not found" });
    }

}