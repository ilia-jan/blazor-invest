using BlazorInvest.Web.Services.Implementations;
using BlazorInvest.Web.Services.Interfaces;
using Microsoft.Extensions.Options;
using Tinkoff.InvestApi;

namespace BlazorInvest.Web.Infrastructure.Extensions;

public static class WebApplicationBuilderExtensions
{
    
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<InvestApiSettings>(builder.Configuration.GetSection("InvestApi"));
        builder.Services.AddInvestApiClient("InvestApiClient", (services, settings) =>
        {
            var apiSettings = services.GetRequiredService<IOptions<InvestApiSettings>>().Value;

            settings.AccessToken = apiSettings.AccessToken;
            settings.Sandbox = apiSettings.Sandbox;
            settings.AppName = apiSettings.AppName;
        });

        builder.Services.AddScoped<ICouponsService, CouponsService>();
    }
}