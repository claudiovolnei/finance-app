using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class DeleteTransactionUseCase
{
    private readonly ITransactionRepository _repository;

    public DeleteTransactionUseCase(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with id {id} not found");

        await _repository.DeleteAsync(id);
    }
}
