namespace AuthService.Models.Response;

/// <summary>DTO nhẹ cho dropdown/select — chỉ chứa Id và Name.</summary>
public class RoleDropdownDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
