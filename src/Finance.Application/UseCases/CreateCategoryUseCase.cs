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

    public async Task<Category> ExecuteAsync(string name)
    {
        var category = new Category(name);
        await _repository.AddAsync(category);
        return category;
    }
}
