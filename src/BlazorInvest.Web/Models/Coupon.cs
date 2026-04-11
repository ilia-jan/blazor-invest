namespace BlazorInvest.Web.Models;

public record Coupon(string Title, DateTime When, decimal Amount, long Quantity);