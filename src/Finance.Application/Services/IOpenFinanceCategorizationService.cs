using Finance.Application.Models.OpenFinance;
using Finance.Domain.Entities;

namespace Finance.Application.Services;

public interface IOpenFinanceCategorizationService
{
    CategorizedOpenFinanceTransactionModel SuggestCategory(
        CreditCardTransactionModel transaction,
        IReadOnlyList<Category> categories);
}
