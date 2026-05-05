using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using CommonService.Common;
using CommonService.Filters;

namespace AggregatorService.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class ProductPageController : ControllerBase
{
    private readonly IProductPageAggregatorService _productPageService;

    public ProductPageController(IProductPageAggregatorService productPageService)
    {
        _productPageService = productPageService;
    }

    // GET /api/v1/admin/products-page?pageNumber=1&pageSize=10&...
    [HttpGet("products-page")]
    [RequiresPermission("GET", "/api/v1/admin/products-page")]
    public async Task<IActionResult> GetProductPage([FromQuery] ProductPageRequest request)
    {
        var result = await _productPageService.GetProductPageAsync(request);

        return StatusCode(result.StatusCode, new ApiResponse<object>
        {
            StatusCode = result.StatusCode,
            Error = result.Error,
            Message = result.Message,
            Data = result.Data
        });
    }
}
