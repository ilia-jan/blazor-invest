using Google.Protobuf.WellKnownTypes;

namespace BlazorInvest.Web.Models;

public record Account(string Name, DateOnly OpenedAt, 
    decimal Total,
    decimal TotalYieldRelative,
    decimal DailyYield,
    decimal DailyYieldRelative
);