using Finance.Domain.Entities;
using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class UpdateTransactionUseCase
{
    private readonly ITransactionRepository _repository;

    public UpdateTransactionUseCase(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        Guid id,
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with id {id} not found");

        await _repository.UpdateAsync(transaction, accountId, categoryId, amount, date, description, type);
    }
}
