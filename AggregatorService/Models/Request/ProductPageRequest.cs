namespace AggregatorService.Models.Request;

public class ProductPageRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? CategoryId { get; set; }
    public int? Status { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}
