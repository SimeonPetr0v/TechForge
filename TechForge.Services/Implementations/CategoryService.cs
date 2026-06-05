using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Data;
using TechForge.Services.Contracts;

namespace TechForge.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoryService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryInputDto?> GetForEditAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category is null ? null : _mapper.Map<CategoryInputDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryInputDto input)
    {
        var category = _mapper.Map<Category>(input);
        category.Id = 0;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> UpdateAsync(CategoryInputDto input)
    {
        var category = await _context.Categories.FindAsync(input.Id);
        if (category is null)
        {
            return false;
        }

        category.Name = input.Name;
        category.Description = input.Description;
        category.ImageUrl = input.ImageUrl;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
