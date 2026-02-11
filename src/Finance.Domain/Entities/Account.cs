using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Account : BaseEntity
{
    public string Name { get; private set; }
    public decimal InitialBalance { get; private set; }
    public Guid UserId { get; private set; }

    public Account(string name, decimal initialBalance, Guid userId)
    {
        Name = name;
        InitialBalance = initialBalance;
        UserId = userId;
    }
}