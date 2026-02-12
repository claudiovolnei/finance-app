using Finance.Domain.Entities;
using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class CreateCategoryUseCase
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryUseCase(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Category> ExecuteAsync(string name, Guid userId)
    {
        var category = new Category(name, userId);
        await _repository.AddAsync(category);
        return category;
    }
}
