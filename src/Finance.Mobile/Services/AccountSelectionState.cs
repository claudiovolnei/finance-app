using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

public class AccountSelectionState
{
    public IReadOnlyList<Account> Accounts { get; private set; } = Array.Empty<Account>();
    public int? SelectedAccountId { get; private set; }

    public event Action? OnChange;

    public void SetAccounts(IReadOnlyList<Account> accounts)
    {
        Accounts = accounts;

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
