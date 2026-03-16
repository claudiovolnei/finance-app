using System.Globalization;
using System.Text;
using Finance.Application.Models.OpenFinance;
using Finance.Application.Services;
using Finance.Domain.Entities;

namespace Finance.Infrastructure.OpenFinance;

public sealed class OpenFinanceCategorizationService : IOpenFinanceCategorizationService
{
    private static readonly Dictionary<string, string[]> KeywordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mercado"] = ["mercado", "super", "carrefour", "pao de acucar", "atacadao", "extra"],
        ["alimentacao"] = ["ifood", "restaurante", "lanchonete", "food", "padaria"],
        ["transporte"] = ["uber", "99", "combustivel", "posto", "shell", "ipiranga", "estacionamento"],
        ["saude"] = ["farmacia", "droga", "drogasil", "raia", "hospital", "clinica"],
        ["lazer"] = ["cinema", "netflix", "spotify", "show", "ingresso"],
        ["casa"] = ["energia", "agua", "internet", "telefone", "aluguel", "condominio"],
        ["educacao"] = ["escola", "faculdade", "curso", "udemy", "alura"],
        ["investimentos"] = ["corretora", "tesouro", "invest", "xp", "rico"],
        ["salario"] = ["salario", "folha", "provento", "pagamento"],
        ["transferencia"] = ["pix", "ted", "doc", "transferencia"]
    };

    public CategorizedOpenFinanceTransactionModel SuggestCategory(
        CreditCardTransactionModel transaction,
        IReadOnlyList<Category> categories)
    {
        if (categories.Count == 0)
        {
            return new CategorizedOpenFinanceTransactionModel(transaction, null, null, 0.10m, "No user categories available.");
        }

        var normalizedDescription = Normalize(transaction.Description);

        var exactMatch = categories.FirstOrDefault(c => normalizedDescription.Contains(Normalize(c.Name), StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            return new CategorizedOpenFinanceTransactionModel(transaction, exactMatch.Id, exactMatch.Name, 0.95m, "Matched by category name in description.");
        }

        foreach (var category in categories)
        {
            var normalizedCategoryName = Normalize(category.Name);
            if (!KeywordMap.TryGetValue(normalizedCategoryName, out var keywords))
            {
                continue;
            }

            if (keywords.Any(keyword => normalizedDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return new CategorizedOpenFinanceTransactionModel(transaction, category.Id, category.Name, 0.80m, "Matched by keyword heuristic.");
            }
        }

        var fallback = categories.FirstOrDefault(c => c.Type == TransactionType.Expense) ?? categories.First();
        return new CategorizedOpenFinanceTransactionModel(transaction, fallback.Id, fallback.Name, 0.35m, "Fallback to first category of matching transaction type.");
    }

    private static string Normalize(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
