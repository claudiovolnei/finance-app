using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public int UserId { get; private set; }

    public Category(string name, int userId)
    {
        Name = name;
        UserId = userId;
    }
}