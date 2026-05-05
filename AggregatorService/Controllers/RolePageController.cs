using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using CommonService.Annotations;
using CommonService.Common;
using CommonService.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AggregatorService.Controllers;

[ApiController]
[Route("api/v1/admin/roles-page")]
[Authorize]
[RequiresPermission("GET", "/api/v1/admin/roles-page")]
public class RolePageController : ControllerBase
{
    private readonly IRolePageAggregatorService _rolePageService;

    public RolePageController(IRolePageAggregatorService rolePageService)
    {
        _rolePageService = rolePageService;
    }

    [HttpGet]
    [ApiMessage("Lấy trang quản lý chức vụ thành công")]
    public async Task<ActionResult<ApiResponse<object>>> GetRolePage([FromQuery] RolePageRequest request)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var (statusCode, data, error, message) = await _rolePageService.GetRolePageAsync(request, token);

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
