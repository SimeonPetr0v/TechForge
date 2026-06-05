using TechForge.Core.Dtos;

namespace TechForge.Services.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryInputDto?> GetForEditAsync(int id);

    Task<CategoryDto> CreateAsync(CategoryInputDto input);

    Task<bool> UpdateAsync(CategoryInputDto input);

    Task<bool> DeleteAsync(int id);
}
