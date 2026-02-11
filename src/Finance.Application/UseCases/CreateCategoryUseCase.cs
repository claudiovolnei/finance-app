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
        // TODO: associate with current user; for now use system user id (Guid.Empty)
        var userId = Guid.Empty;
        var category = new Category(name, userId);
        await _repository.AddAsync(category);
        return category;
    }
}
