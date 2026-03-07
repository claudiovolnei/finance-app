using Finance.Application.Repositories;
using Finance.Domain.Entities;

namespace Finance.Application.UseCases;

public class GetCategoriesByOwnerUserUseCase
{
    private readonly ICategoryRepository _repository;

    public GetCategoriesByOwnerUserUseCase(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Category>> ExecuteAsync(int ownerUserId)
    {
        return await _repository.GetByOwnerUserIdAsync(ownerUserId);
    }
}
