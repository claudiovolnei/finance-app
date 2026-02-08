using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class UpdateCategoryUseCase
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryUseCase(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id, string name)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
            throw new InvalidOperationException($"Category with id {id} not found");

        // O repositório irá atualizar usando EF Core
        await _repository.UpdateAsync(category, name);
    }
}
