using AggregatorService.Models.Request;

namespace AggregatorService.Services.Interfaces;

public interface IProfileAggregatorService
{
    Task<(bool IsSuccess, int StatusCode, object? Data, string? Error, string Message)> UpdateProfileAsync(UpdateProfileAggregatedRequest request, string token, string userId);
}
