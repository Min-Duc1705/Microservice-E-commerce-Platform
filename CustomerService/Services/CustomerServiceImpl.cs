using CommonService.Common;
using CommonService.Exceptions;
using CustomerService.Models;
using CustomerService.Models.Request;
using CustomerService.Models.Response;
using CustomerService.Repository.Interface;
using CustomerService.Services.Interface;
using CustomerService.Specifications;
using CustomerService.Utils.Enum;
using CommonService.Interface;

namespace CustomerService.Services;

public class CustomerServiceImpl : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IMediaService _mediaService;

    public CustomerServiceImpl(
        ICustomerRepository customerRepo,
        IMediaService mediaService)
    {
        _customerRepo = customerRepo;
        _mediaService = mediaService;
    }

    public async Task<ResultPaginationDto<CustomerResponse>> GetPagedCustomersAsync(CustomerFilterRequest filter)
    {
        var spec = new CustomerFilterSpec(
            filter.SearchTerm,
            filter.Status,
            filter.SortBy,
            filter.IsDescending,
            filter.PageNumber,
            filter.PageSize,
            filter.FromDate,
            filter.ToDate,
            filter.IncludeDeleted);

        var countSpec = new CustomerFilterCountSpec(filter.SearchTerm, filter.Status, filter.FromDate, filter.ToDate, filter.IncludeDeleted);

        var customers = await _customerRepo.ListAsync(spec);
        var totalCount = await _customerRepo.CountAsync(countSpec);

        var items = customers.Select(MapToResponse).ToList();

        return new ResultPaginationDto<CustomerResponse>(items, filter.PageNumber, filter.PageSize, totalCount);
    }

    public async Task<CustomerResponse> GetCustomerByIdAsync(Guid id)
    {
        var spec = new CustomerByIdSpec(id, includeDeleted: true);
        var customer = await _customerRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy khách hàng với ID: {id}");

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        // Kiểm tra SĐT trùng
        if (await _customerRepo.PhoneExistsAsync(request.Phone))
            throw new BadRequestException($"Số điện thoại '{request.Phone}' đã tồn tại trong hệ thống.");

        // Kiểm tra Email trùng (nếu có nhập)
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _customerRepo.EmailExistsAsync(request.Email))
            throw new BadRequestException($"Email '{request.Email}' đã tồn tại trong hệ thống.");

        // Commit avatar từ temp/ → customers/ (nếu có)
        var avatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
            ? null
            : await _mediaService.CommitFileAsync(request.AvatarUrl, "customers");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            AvatarUrl = avatarUrl,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepo.AddAsync(customer);
        await _customerRepo.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        var spec = new CustomerByIdSpec(id, includeDeleted: true);
        var customer = await _customerRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy khách hàng với ID: {id}");

        // Kiểm tra SĐT trùng với khách hàng khác
        if (customer.Phone != request.Phone &&
            await _customerRepo.PhoneExistsAsync(request.Phone, excludeId: id))
            throw new BadRequestException($"Số điện thoại '{request.Phone}' đã được dùng bởi khách hàng khác.");

        // Kiểm tra Email trùng
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(customer.Email, request.Email, StringComparison.OrdinalIgnoreCase) &&
            await _customerRepo.EmailExistsAsync(request.Email, excludeId: id))
            throw new BadRequestException($"Email '{request.Email}' đã được dùng bởi khách hàng khác.");

        customer.FullName = request.FullName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Address = request.Address;

        // Commit avatar mới nếu có (nếu URL đã là "customers/" thì CommitFileAsync trả lại nguyên)
        customer.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
            ? null
            : await _mediaService.CommitFileAsync(request.AvatarUrl, "customers");

        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var spec = new CustomerByIdSpec(id, includeDeleted: true);
        var customer = await _customerRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy khách hàng với ID: {id}");

        // Soft delete — không xóa vật lý (theo Impl.md)
        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();
    }

    public async Task ToggleBlockCustomerAsync(Guid id, CustomerStatus newStatus)
    {
        var spec = new CustomerByIdSpec(id, includeDeleted: true);
        var customer = await _customerRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy khách hàng với ID: {id}");

        if (customer.Status == newStatus)
            throw new BadRequestException($"Khách hàng đã ở trạng thái '{newStatus}'.");

        customer.Status = newStatus;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();
    }

    private CustomerResponse MapToResponse(Customer c) => new()
    {
        Id = c.Id,
        FullName = c.FullName,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        Status = c.Status.ToString(),
        AvatarUrl = c.AvatarUrl,
        TotalSpent = c.TotalSpent,
        DebtAmount = c.DebtAmount,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
