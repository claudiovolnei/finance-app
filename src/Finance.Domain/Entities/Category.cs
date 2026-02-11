using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public Guid UserId { get; private set; }

    public Category(string name, Guid userId)
    {
        Name = name;
        UserId = userId;
    }
}