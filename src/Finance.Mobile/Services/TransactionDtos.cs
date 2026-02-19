namespace Finance.Mobile.Services;

public record TransactionDto(
    int Id,
    int AccountId,
    int CategoryId,
    string CategoryName,
    decimal Amount,
    DateTime Date,
    string? Description,
    Finance.Domain.Entities.TransactionType Type);
