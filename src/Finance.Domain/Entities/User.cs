using Finance.Domain.Common;

namespace Finance.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string PasswordSalt { get; private set; }

    public User(string username, string passwordHash, string passwordSalt)
    {
        Username = username;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }
}
