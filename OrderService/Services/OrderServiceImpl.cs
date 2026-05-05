using CommonService.Common;
using CommonService.Events;
using MassTransit;
using OrderService.Models;
using OrderService.Models.Request;
using OrderService.Models.Response;
using OrderService.Repository.Interface;
using OrderService.Services.Interface;
using OrderService.Specifications;
using OrderService.Utils.Enum;
using OrderService.Data;

namespace OrderService.Services;

public class OrderServiceImpl : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly OrderDbContext _dbContext;

    public OrderServiceImpl(IOrderRepository orderRepo, IPublishEndpoint publishEndpoint, OrderDbContext dbContext)
    {
        _orderRepo = orderRepo;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }

    public async Task<ResultPaginationDto<OrderResponse>> GetPagedOrdersAsync(OrderFilterRequest filter)
    {
        // Fix PostgreSQL: Chuyển DateTime từ Unspecified sang UTC
        var fromDate = filter.FromDate.HasValue
            ? DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;
        var toDate = filter.ToDate.HasValue
            ? DateTime.SpecifyKind(filter.ToDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        // 1. Tạo Specification để filter + sort + paging
        var spec = new OrderFilterSpec(
            filter.CustomerId,
            filter.Status,
            fromDate,
            toDate,
            filter.SearchTerm,
            filter.SortBy,
            filter.IsDescending,
            filter.PageNumber,
            filter.PageSize);

        // 2. Tạo Specification chỉ để đếm (không paging)
        var countSpec = new OrderFilterCountSpec(
            filter.CustomerId,
            filter.Status,
            fromDate,
            toDate,
            filter.SearchTerm);

        // 3. Thực thi 2 query song song: lấy data + đếm tổng
        var orders = await _orderRepo.ListAsync(spec);
        var totalCount = await _orderRepo.CountAsync(countSpec);

        // 4. Map sang DTO và đóng gói vào ResultPaginationDto
        var responseItems = orders.Select(MapToResponse).ToList();

        return new ResultPaginationDto<OrderResponse>(
            responseItems,
            filter.PageNumber,
            filter.PageSize,
            totalCount);
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid id)
    {
        // Dùng Specification để lấy Order kèm Items
        var spec = new OrderWithItemsSpec(id);
        var order = await _orderRepo.GetEntityWithSpec(spec);

        if (order == null)
            throw new CommonService.Exceptions.NotFoundException($"Không tìm thấy đơn hàng với mã ID: {id}");

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid? customerId)
    {
        string finalCustomerName = request.CustomerName;

        // Auto filter real FullName from CustomerProfile based on Auth ID
        if (customerId.HasValue && customerId.Value != Guid.Empty)
        {
            var profile = await _dbContext.Set<CustomerProfile>().FindAsync(customerId.Value);
            if (profile != null && !string.IsNullOrWhiteSpace(profile.FullName))
            {
                finalCustomerName = profile.FullName;
            }
        }

        var newOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerName = finalCustomerName,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = request.ShippingAddress,
            PaymentMethod = request.PaymentMethod,
            ShippingFee = request.ShippingFee,
            Note = request.Note,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow,

            Items = request.Items.Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        // Bắn OrderCreatedEvent lên RabbitMQ (Trước SaveChanges để Outbox bắt được)
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = newOrder.Id,
            CustomerId = newOrder.CustomerId,
            CustomerName = newOrder.CustomerName,
            CustomerPhone = newOrder.CustomerPhone,
            SubTotal = newOrder.SubTotal,
            ShippingFee = newOrder.ShippingFee,
            TotalAmount = newOrder.TotalAmount,
            PaymentMethod = newOrder.PaymentMethod.ToString(),
            ProcessedAt = newOrder.CreatedAt,
            Items = newOrder.Items.Select(i => new OrderItemInfo
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                UnitCost = i.UnitCost
            }).ToList()
        });

        await _orderRepo.SaveChangesAsync();

        return MapToResponse(newOrder);
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        // Dùng Specification để lấy Order kèm Items
        var spec = new OrderWithItemsSpec(orderId);
        var order = await _orderRepo.GetEntityWithSpec(spec);

        if (order == null)
            throw new CommonService.Exceptions.NotFoundException($"Không thể cập nhật: Không tìm thấy đơn hàng mã {orderId}");

        var oldStatus = order.Status;
        order.Status = newStatus;
        _orderRepo.Update(order);
        
        // 1. Bắn sự kiện thay đổi trạng thái chung (cho CustomerService...)
        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OldStatus = oldStatus.ToString(),
            NewStatus = newStatus.ToString(),
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            ProcessedAt = DateTime.UtcNow,
            Items = order.Items.Select(i => new OrderItemInfo
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                UnitCost = i.UnitCost
            }).ToList()
        });

        // 2. Nếu chuyển sang Completed -> Bắn thêm OrderCompletedEvent cho ReportService
        if (newStatus == OrderStatus.Completed)
        {
            await _publishEndpoint.Publish(new OrderCompletedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CompletedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Items = order.Items.Select(i => new OrderItemInfo
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    UnitCost = i.UnitCost
                }).ToList()
            });
        }

        await _orderRepo.SaveChangesAsync();
        return MapToResponse(order);
    }

    public async Task UpdateOrderStatusIfNewAsync(Guid orderId, OrderStatus newStatus)
    {
        var spec = new OrderWithItemsSpec(orderId);
        var order = await _orderRepo.GetEntityWithSpec(spec);

        // Không tìm thấy hoặc không phải New → bỏ qua (tránh lỗi khi event tới trễ)
        if (order == null || order.Status != OrderStatus.New)
            return;

        var oldStatus = order.Status;
        order.Status = newStatus;
        _orderRepo.Update(order);

        // 1. Status Changed Event
        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OldStatus = oldStatus.ToString(),
            NewStatus = newStatus.ToString(),
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            ProcessedAt = DateTime.UtcNow,
            Items = order.Items.Select(i => new OrderItemInfo
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                UnitCost = i.UnitCost
            }).ToList()
        });

        // 2. Order Completed Event (if applicable)
        if (newStatus == OrderStatus.Completed)
        {
            await _publishEndpoint.Publish(new OrderCompletedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CompletedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Items = order.Items.Select(i => new OrderItemInfo
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    UnitCost = i.UnitCost
                }).ToList()
            });
        }

        await _orderRepo.SaveChangesAsync();
    }

    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),

            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
