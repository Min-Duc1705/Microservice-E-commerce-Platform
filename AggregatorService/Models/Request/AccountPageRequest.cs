namespace AggregatorService.Models.Request;

public class AccountPageRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public Guid? RoleId { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
}
