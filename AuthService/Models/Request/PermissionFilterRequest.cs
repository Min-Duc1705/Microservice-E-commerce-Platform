namespace AuthService.Models.Request;

/// <summary>Filter + phân trang cho GET /api/v1/permissions</summary>
public class PermissionFilterRequest
{
    private int _pageSize = 20;
    private const int MaxPageSize = 200;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SearchTerm { get; set; }
    public string? Module { get; set; }
    public string? Method { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
}
