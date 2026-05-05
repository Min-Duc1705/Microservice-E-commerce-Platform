using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using CommonService.Annotations;
using CommonService.Common;
using CommonService.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AggregatorService.Controllers;

[ApiController]
[Route("api/v1/admin/accounts-page")]
[RequiresPermission("GET", "/api/v1/admin/accounts-page")]
public class AccountPageController : ControllerBase
{
    private readonly IAccountPageAggregatorService _accountPageService;

    public AccountPageController(IAccountPageAggregatorService accountPageService)
    {
        _accountPageService = accountPageService;
    }

    [HttpGet]
    [ApiMessage("Lấy trang quản lý tài khoản thành công")]
    public async Task<ActionResult<ApiResponse<object>>> GetAccountPage([FromQuery] AccountPageRequest request)
    {
        // Trích xuất Token từ Authorization header của request hiện tại để forward xuống AuthService
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var (statusCode, data, error, message) = await _accountPageService.GetAccountPageAsync(request, token);

        var response = new ApiResponse<object>
        {
            StatusCode = statusCode,
            Message = message,
            Error = error,
            Data = data
        };

        if (statusCode >= 400)
        {
            return StatusCode(statusCode, response);
        }

        return Ok(response);
    }
}
