using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public TransactionType Type { get; private set; }
    public int UserId { get; private set; }

    public Category(string name, int userId, TransactionType type)
    {
        Name = name;
        UserId = userId;
        Type = type;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }
}
