using AggregatorService.Models.Request;

namespace AggregatorService.Services.Interfaces;

public interface IRolePageAggregatorService
{
    Task<(int StatusCode, object? Data, string? Error, string Message)> GetRolePageAsync(RolePageRequest request, string token);
}
