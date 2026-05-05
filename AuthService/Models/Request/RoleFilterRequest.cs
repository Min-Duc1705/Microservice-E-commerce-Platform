namespace AuthService.Models.Request;

public class RoleFilterRequest
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
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
}
