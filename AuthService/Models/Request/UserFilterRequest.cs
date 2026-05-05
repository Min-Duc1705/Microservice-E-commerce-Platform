namespace AuthService.Models.Request;

public class UserFilterRequest
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>Tìm theo username hoặc email.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Lọc theo trạng thái kích hoạt.</summary>
    public bool? IsActive { get; set; }

    /// <summary>Lọc theo Role ID.</summary>
    public Guid? RoleId { get; set; }

    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
}
