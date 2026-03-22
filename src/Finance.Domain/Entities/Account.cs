using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Account : BaseEntity
{
    public string Name { get; private set; }
    public decimal InitialBalance { get; private set; }
    public int UserId { get; private set; }
    public AccountType Type { get; private set; }
    public int? ParentAccountId { get; private set; }

    public Account(string name, decimal initialBalance, int userId, AccountType type = AccountType.Checking, int? parentAccountId = null)
    {
        Name = name;
        InitialBalance = initialBalance;
        UserId = userId;
        Type = type;
        ParentAccountId = parentAccountId;
    }

    public void Update(string name, decimal initialBalance, AccountType type, int? parentAccountId)
    {
        Name = name;
        InitialBalance = initialBalance;
        Type = type;
        ParentAccountId = parentAccountId;
    }
}

public enum AccountType
{
    Checking,
    CreditCard
}
