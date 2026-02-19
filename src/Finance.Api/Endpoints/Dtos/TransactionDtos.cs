using Finance.Domain.Entities;

namespace Finance.Api.Endpoints.Dtos;

public record CreateTransactionRequest(
    int AccountId,
    int CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);

public record UpdateTransactionRequest(
    int AccountId,
    int CategoryId,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);

// DTO returned to clients including category name for display
public record TransactionResponseDto(
    int Id,
    int AccountId,
    int CategoryId,
    string CategoryName,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);
