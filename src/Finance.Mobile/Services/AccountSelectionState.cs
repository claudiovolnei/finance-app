using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

public class AccountSelectionState
{
    public IReadOnlyList<Account> AllAccounts { get; private set; } = Array.Empty<Account>();
    public IReadOnlyList<Account> Accounts { get; private set; } = Array.Empty<Account>();
    public int? SelectedAccountId { get; private set; }

    public event Action? OnChange;

    public IEnumerable<Account> CreditCardsForSelectedAccount => SelectedAccountId.HasValue
        ? AllAccounts.Where(a => a.Type == AccountType.CreditCard && a.ParentAccountId == SelectedAccountId.Value)
        : Enumerable.Empty<Account>();

    public void SetAccounts(IReadOnlyList<Account> accounts)
    {
        AllAccounts = accounts;
        Accounts = accounts.Where(a => a.Type == AccountType.Checking).OrderBy(a => a.Name).ToList();

        if (Accounts.Count == 0)
        {
            SelectedAccountId = null;
        }
        else if (!SelectedAccountId.HasValue || Accounts.All(a => a.Id != SelectedAccountId.Value))
        {
            SelectedAccountId = Accounts[0].Id;
        }

        OnChange?.Invoke();
    }

    public void SetSelectedAccount(int? accountId)
    {
        if (accountId.HasValue && Accounts.All(a => a.Id != accountId.Value))
            return;

        if (SelectedAccountId == accountId)
            return;

        SelectedAccountId = accountId;
        OnChange?.Invoke();
    }
}
