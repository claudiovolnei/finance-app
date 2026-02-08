using Finance.Domain.Entities;

namespace Finance.Api.Endpoints.Dtos;

public record CreateTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);

public record UpdateTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);
