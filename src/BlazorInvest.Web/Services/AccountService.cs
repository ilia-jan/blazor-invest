using System.Collections.Concurrent;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using Account = BlazorInvest.Web.Models.Account;

namespace BlazorInvest.Web.Services;

public class AccountService(InvestApiClient investApiClient) : IAccountService
{
    public async Task<IReadOnlyCollection<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accountsResponse = await investApiClient.Users.GetAccountsAsync(new GetAccountsRequest(),
            cancellationToken: cancellationToken);

        var brokerAccounts = accountsResponse.Accounts
            .Where(a => a.Status is AccountStatus.Open && a.Type is AccountType.Tinkoff or AccountType.TinkoffIis)
            .ToArray();

        var accounts = new ConcurrentBag<Account>();
        await Parallel.ForEachAsync(brokerAccounts,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = brokerAccounts.Length,
                CancellationToken = cancellationToken
            },
            async (account, ct) =>
            {
                var portfolioRequest = new PortfolioRequest { AccountId = account.Id };
                var portfolio = await investApiClient.Operations.GetPortfolioAsync(portfolioRequest, cancellationToken: ct);
                
                accounts.Add(new Account(
                    account.Name, 
                    DateOnly.FromDateTime(account.OpenedDate.ToDateTime()),
                    portfolio.TotalAmountPortfolio,
                    portfolio.ExpectedYield,
                    portfolio.DailyYield,
                    portfolio.DailyYieldRelative
                ));
            });

        return accounts.ToArray();
    }
}