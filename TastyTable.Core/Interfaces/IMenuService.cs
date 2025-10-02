using TastyTable.Core.DTOs;
using TastyTable.Core.Entities;

namespace TastyTable.Core.Interfaces;

public interface IMenuService
{
    Task<MenuItem> CreateAsync(MenuItemCreateDto dto);
    Task<IEnumerable<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByIdAsync(int id);
    Task<MenuItem?> UpdateAvailabilityAsync(int id, bool isAvailable);
    Task<bool> DeleteAsync(int id);
}