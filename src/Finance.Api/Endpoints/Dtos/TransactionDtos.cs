using Finance.Domain.Entities;

namespace Finance.Api.Endpoints.Dtos;

public record CreateTransactionRequest(
    int AccountId,
    int? CategoryId,
    int? TransferAccountId,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);

public record UpdateTransactionRequest(
    int AccountId,
    int? CategoryId,
    int? TransferAccountId,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);

public record TransactionResponseDto(
    int Id,
    int AccountId,
    int? CategoryId,
    int? TransferAccountId,
    string TransactionAccountName,
    string CategoryName,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);
