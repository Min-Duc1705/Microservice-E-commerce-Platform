using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using CommonService.Common;
using System.Security.Claims;

namespace AggregatorService.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileAggregatorService _profileService;

    public ProfileController(IProfileAggregatorService profileService)
    {
        _profileService = profileService;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileAggregatedRequest request)
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(authHeader) || string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiResponse<object>
            {
                StatusCode = 401,
                Error = "Unauthorized",
                Message = "Unauthorized access.",
                Data = null
            });
        }

        var result = await _profileService.UpdateProfileAsync(request, authHeader, userId);

        return StatusCode(result.StatusCode, new ApiResponse<object>
        {
            StatusCode = result.StatusCode,
            Error = result.Error,
            Message = result.Message,
            Data = result.Data
        });
    }
}
