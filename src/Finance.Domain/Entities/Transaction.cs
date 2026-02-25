using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Transaction : BaseEntity
{
    public int AccountId { get; private set; }
    public int CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string Description { get; private set; }
    public TransactionType Type { get; private set; }
    public int UserId { get; private set; }

    public Transaction(
        int accountId,
        int categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type,
        int userId)
    {
        AccountId = accountId;
        CategoryId = categoryId;
        Amount = amount;
        Date = date;
        Description = description;
        Type = type;
        UserId = userId;
    }

    public void Update(
        int accountId,
        int categoryId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type)
    {
        AccountId = accountId;
        CategoryId = categoryId;
        Amount = amount;
        Date = date;
        Description = description;
        Type = type;
    }
}

public enum TransactionType
{
    Income,
    Expense
}
