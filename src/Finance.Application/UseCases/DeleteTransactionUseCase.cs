using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class DeleteTransactionUseCase
{
    private readonly ITransactionRepository _repository;

    public DeleteTransactionUseCase(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(int id, int userId)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with id {id} not found");

        if (transaction.UserId != userId)
            throw new InvalidOperationException("Not authorized");

        await _repository.DeleteAsync(id);
    }
}
