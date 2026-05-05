using CommonService.Annotations;
using CommonService.Common;
using CustomerService.Models.Request;
using CustomerService.Models.Response;
using CustomerService.Services.Interface;
using CustomerService.Utils.Enum;
using Microsoft.AspNetCore.Mvc;
using CommonService.Filters;

namespace CustomerService.Controllers;

[ApiController]
[Route("api/v1/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // GET: /api/customers?searchTerm=Nguyen&status=0&pageNumber=1&pageSize=10
    [HttpGet]
    [RequiresPermission("GET", "/api/v1/customers")]
    [ApiMessage("Lấy danh sách khách hàng thành công")]
    public async Task<ActionResult<ResultPaginationDto<CustomerResponse>>> GetAllCustomers(
        [FromQuery] CustomerFilterRequest filter)
    {
        var result = await _customerService.GetPagedCustomersAsync(filter);
        return Ok(result);
    }

    // GET: /api/customers/{id}
    [HttpGet("{id}")]
    [RequiresPermission("GET", "/api/v1/customers/{id}")]
    [ApiMessage("Lấy thông tin khách hàng thành công")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        return Ok(customer);
    }

    // POST: /api/customers
    [HttpPost]
    [RequiresPermission("POST", "/api/v1/customers")]
    [ApiMessage("Tạo khách hàng mới thành công")]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(
        [FromBody] CreateCustomerRequest request)
    {
        var customer = await _customerService.CreateCustomerAsync(request);
        return StatusCode(201, customer);
    }

    // PUT: /api/customers/{id}
    [HttpPut("{id}")]
    [RequiresPermission("PUT", "/api/v1/customers/{id}")]
    [ApiMessage("Cập nhật thông tin khách hàng thành công")]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(
        Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var customer = await _customerService.UpdateCustomerAsync(id, request);
        return Ok(customer);
    }

    // DELETE: /api/customers/{id}  (Soft delete)
    [HttpDelete("{id}")]
    [RequiresPermission("DELETE", "/api/v1/customers/{id}")]
    [ApiMessage("Xóa khách hàng thành công")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return Ok(null);
    }

    // PATCH: /api/customers/{id}/status
    [HttpPatch("{id}/status")]
    [RequiresPermission("PATCH", "/api/v1/customers/{id}/status")]
    [ApiMessage("Cập nhật trạng thái khách hàng thành công")]
    public async Task<IActionResult> ToggleBlockCustomer(
        Guid id, [FromBody] CustomerStatus newStatus)
    {
        await _customerService.ToggleBlockCustomerAsync(id, newStatus);
        return Ok(null);
    }
}
