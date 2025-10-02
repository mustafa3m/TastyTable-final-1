namespace TastyTable.Core.DTOs;

public record MenuItemCreateDto(string Name, string Description, decimal Price, bool IsAvailable);
public record MenuItemReadDto(int Id, string Name, string Description, decimal Price, bool IsAvailable);