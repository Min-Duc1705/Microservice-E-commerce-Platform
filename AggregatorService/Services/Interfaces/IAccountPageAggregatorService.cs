using AggregatorService.Models.Request;

namespace AggregatorService.Services.Interfaces;

public interface IAccountPageAggregatorService
{
    Task<(int StatusCode, object? Data, string? Error, string Message)> GetAccountPageAsync(AccountPageRequest request, string token);
}
