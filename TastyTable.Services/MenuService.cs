using Microsoft.EntityFrameworkCore;
using TastyTable.Core.DTOs;
using TastyTable.Core.Entities;
using TastyTable.Core.Interfaces;
using TastyTable.Data;

namespace TastyTable.Services;

public class MenuService : IMenuService
{
    private readonly AppDbContext _context;
    public MenuService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<MenuItem>> GetAllAsync() =>
        await _context.MenuItems.AsNoTracking().ToListAsync();

    public async Task<MenuItem?> GetByIdAsync(int id) =>
        await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<MenuItem> CreateAsync(MenuItemCreateDto dto)
    {
        var item = new MenuItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = dto.IsAvailable
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<MenuItem?> UpdateAvailabilityAsync(int id, bool isAvailable)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item is null) return null;

        item.IsAvailable = isAvailable;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.MenuItems.FindAsync(id);
        if (entity is null) return false;

        _context.MenuItems.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
