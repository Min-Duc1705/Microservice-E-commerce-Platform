using CommonService.Common;
using CustomerService.Models.Request;
using CustomerService.Models.Response;
using CustomerService.Utils.Enum;

namespace CustomerService.Services.Interface;

public interface ICustomerService
{
    /// <summary>Lấy danh sách khách hàng có filter + phân trang</summary>
    Task<ResultPaginationDto<CustomerResponse>> GetPagedCustomersAsync(CustomerFilterRequest filter);

    /// <summary>Lấy chi tiết 1 khách hàng theo ID</summary>
    Task<CustomerResponse> GetCustomerByIdAsync(Guid id);

    /// <summary>Tạo mới khách hàng</summary>
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);

    /// <summary>Cập nhật thông tin khách hàng</summary>
    Task<CustomerResponse> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request);

    /// <summary>Soft delete khách hàng (theo Impl.md: không xóa vĩnh viễn)</summary>
    Task DeleteCustomerAsync(Guid id);

    /// <summary>Khóa / Mở khóa tài khoản khách hàng</summary>
    Task ToggleBlockCustomerAsync(Guid id, CustomerStatus newStatus);
}
