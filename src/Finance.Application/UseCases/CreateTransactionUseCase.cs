using Finance.Domain.Entities;
using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class CreateTransactionUseCase
{
    private readonly ITransactionRepository _repository;

    public CreateTransactionUseCase(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type)
    {
        var transaction = new Transaction(
            accountId,
            categoryId,
            amount,
            date,
            description,
            type);

        await _repository.AddAsync(transaction);
    }
}