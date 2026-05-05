using CommonService.Repository;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Repository.Interface;
using PaymentService.Utils.Enum;

namespace PaymentService.Repository;

public class PaymentRepository : GenericRepository<PaymentDbContext, PaymentTransaction>, IPaymentRepository
{
    public PaymentRepository(PaymentDbContext context) : base(context) { }

    public async Task<bool> HasSuccessfulPaymentAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .AnyAsync(t => t.OrderId == orderId && t.Status == PaymentStatus.Success);
    }

    public async Task<PaymentTransaction?> GetLatestByOrderIdAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Where(t => t.OrderId == orderId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
