using System.Text.Json.Serialization;

namespace Finance.Domain.Common;

public abstract class BaseEntity
{
    [JsonInclude]
    public int Id { get; protected set; }
    [JsonInclude]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
