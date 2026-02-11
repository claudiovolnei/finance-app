using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string Description { get; private set; }
    public TransactionType Type { get; private set; }
    public Guid UserId { get; private set; }

    public Transaction(
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type,
        Guid userId)
    {
        AccountId = accountId;
        CategoryId = categoryId;
        Amount = amount;
        Date = date;
        Description = description;
        Type = type;
        UserId = userId;
    }
}

public enum TransactionType
{
    Income,
    Expense
}