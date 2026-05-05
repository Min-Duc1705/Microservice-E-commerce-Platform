using CommonService.Common;

namespace ProductService.Models.Request;

public class CategoryFilterRequest
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SearchTerm { get; set; }
    
    public bool IncludeDeleted { get; set; } = false;

    // Sorting
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = true;
}
