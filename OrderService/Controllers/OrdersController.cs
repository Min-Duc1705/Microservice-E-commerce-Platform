using CommonService.Annotations;
using CommonService.Common;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models.Request;
using OrderService.Models.Response;
using OrderService.Services.Interface;
using OrderService.Utils.Enum;

using System.Security.Claims;
using CommonService.Filters;

namespace OrderService.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: /api/orders?pageNumber=1&pageSize=10&status=Processing&searchTerm=Nguyen
    [HttpGet]
    [RequiresPermission("GET", "/api/v1/orders")]
    [ApiMessage("Lấy danh sách đơn hàng thành công")]
    public async Task<ActionResult<ResultPaginationDto<OrderResponse>>> GetAllOrders([FromQuery] OrderFilterRequest filter)
    {
        var result = await _orderService.GetPagedOrdersAsync(filter);
        return Ok(result);
    }

    // GET: /api/orders/{id}
    [HttpGet("{id}")]
    [RequiresPermission("GET", "/api/v1/orders/{id}")]
    [ApiMessage("Lấy thông tin đơn hàng thành công")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    // POST: /api/orders
    [HttpPost]
    [RequiresPermission("POST", "/api/v1/orders")]
    [ApiMessage("Tạo đơn hàng thành công")]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Đọc trực tiếp từ JWT Claims do Middleware tự giải mã
        Guid? customerId = null;
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdString, out var parsedId))
            customerId = parsedId;

        var newOrder = await _orderService.CreateOrderAsync(request, customerId);
        return StatusCode(201, newOrder);
    }

    // PUT: /api/orders/{id}/status
    [HttpPut("{id}/status")]
    [RequiresPermission("PUT", "/api/v1/orders/{id}/status")]
    [ApiMessage("Cập nhật trạng thái đơn hàng thành công")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatus newStatus)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);
        return Ok(result);
    }
}
