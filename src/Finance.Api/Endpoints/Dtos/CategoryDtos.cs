using Finance.Domain.Entities;

namespace Finance.Api.Endpoints.Dtos;

public record CreateCategoryRequest(string Name, TransactionType Type, int? OwnerUserId = null);
public record UpdateCategoryRequest(string Name);
