using System.Collections.Immutable;
using BlazorInvest.Web.Models;
using BlazorInvest.Web.Services.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using Coupon = BlazorInvest.Web.Models.Coupon;

namespace BlazorInvest.Web.Services.Implementations;

public class CouponsService(InvestApiClient investApiClient) : ICouponsService
{
    private const string BondInstrumentType = "bond";

    public async Task<IReadOnlyCollection<AccountCoupons>> GetCouponsByAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accountsResponse = await investApiClient.Users.GetAccountsAsync(new GetAccountsRequest(),
            cancellationToken: cancellationToken);

        var bondsDictionary = new List<AccountCoupons>();
        var brokerAccounts = accountsResponse.Accounts
            .Where(a => a.Status is AccountStatus.Open && a.Type is AccountType.Tinkoff or AccountType.TinkoffIis)
            .ToArray();
        foreach (var account in brokerAccounts)
        {
            var portfolioRequest = new PortfolioRequest { AccountId = account.Id };
            var portfolioResponse = await investApiClient.Operations.GetPortfolioAsync(portfolioRequest, cancellationToken: cancellationToken);

            var positions = portfolioResponse.Positions
                .Where(p => p.InstrumentType.Equals(BondInstrumentType))
                .ToImmutableArray();
            
            var now = DateTime.UtcNow;
            var from = Timestamp.FromDateTime(now);
            var to = Timestamp.FromDateTime(now.AddMonths(6));
            
            var coupons = new List<Coupon>();
            foreach (var position in positions)
            {
                var instrumentResponse = await investApiClient.Instruments.GetInstrumentByAsync(
                    new InstrumentRequest
                    {
                        Id = position.PositionUid,
                        IdType = InstrumentIdType.PositionUid
                    },
                    cancellationToken: cancellationToken);

                var couponsResponse = await investApiClient.Instruments.GetBondCouponsAsync(new GetBondCouponsRequest
                {
                    Figi = position.Figi,
                    From = from,
                    To = to
                }, cancellationToken: cancellationToken);

                coupons.AddRange(
                    couponsResponse.Events.Select(c => 
                        new Coupon(instrumentResponse.Instrument.Name,
                            c.CouponDate.ToDateTime(),
                            c.PayOneBond.Units + c.PayOneBond.Nano / 1_000_000_000m,
                            position.Quantity.Units)
                    )
                );
            }
            
            bondsDictionary.Add(new AccountCoupons(account.Name, coupons.OrderBy(c => c.When).ToArray()));
        }

        return bondsDictionary.AsReadOnly();
    }
}
