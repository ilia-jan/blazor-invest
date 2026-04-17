using BlazorInvest.Web.Models;

namespace BlazorInvest.Web.Services;

public interface IAccountService
{
    Task<IReadOnlyCollection<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
}