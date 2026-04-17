using BlazorInvest.Web.Models;

namespace BlazorInvest.Web.Services;

public interface ICouponService
{
    Task<IReadOnlyCollection<AccountCoupons>> GetCouponsByAccountsAsync(CancellationToken cancellationToken = default);
}