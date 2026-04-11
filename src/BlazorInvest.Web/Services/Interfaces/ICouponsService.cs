using BlazorInvest.Web.Models;

namespace BlazorInvest.Web.Services.Interfaces;

public interface ICouponsService
{
    Task<IReadOnlyCollection<AccountCoupons>> GetCouponsByAccountsAsync(CancellationToken cancellationToken = default);
}