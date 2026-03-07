using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public TransactionType Type { get; private set; }
    public int UserId { get; private set; }
    public int OwnerUserId { get; private set; }

    public Category(string name, int userId, TransactionType type, int? ownerUserId = null)
    {
        Name = name;
        UserId = userId;
        OwnerUserId = ownerUserId ?? userId;
        Type = type;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }
}
