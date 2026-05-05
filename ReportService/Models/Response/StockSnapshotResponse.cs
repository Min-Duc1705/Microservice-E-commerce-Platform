namespace ReportService.Models.Response;

public class StockSnapshotResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public int SoldLast30Days { get; set; }
    public DateTime? LastSoldAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
