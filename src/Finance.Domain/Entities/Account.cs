using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Account : BaseEntity
{
    public string Name { get; private set; }
    public decimal InitialBalance { get; private set; }
    public int UserId { get; private set; }

    public Account(string name, decimal initialBalance, int userId)
    {
        Name = name;
        InitialBalance = initialBalance;
        UserId = userId;
    }

    public void Update(string name, decimal initialBalance)
    {
        Name = name;
        InitialBalance = initialBalance;
    }
}
