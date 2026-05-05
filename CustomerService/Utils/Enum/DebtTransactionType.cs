namespace CustomerService.Utils.Enum;

public enum DebtTransactionType
{
    /// <summary>Khách mua hàng và trả sau (COD chưa thu) — tạo ra nợ mới</summary>
    NewDebt = 0,

    /// <summary>Khách thanh toán nợ — giảm số dư nợ</summary>
    Payment = 1,
}
