using AggregatorService.Models.Request;

namespace AggregatorService.Services.Interfaces;

public interface IProductPageAggregatorService
{
    Task<(int StatusCode, object? Data, string? Error, string Message)> GetProductPageAsync(ProductPageRequest request);
}
