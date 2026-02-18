using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class DeleteCategoryUseCase
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryUseCase(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(int id, int userId)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
            throw new InvalidOperationException($"Category with id {id} not found");

        if (category.UserId != userId)
            throw new InvalidOperationException("Not authorized");

        await _repository.DeleteAsync(id);
    }
}
