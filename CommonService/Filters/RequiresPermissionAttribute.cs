namespace CommonService.Filters;

/// <summary>
/// Đánh dấu Controller/Action cần kiểm tra Permission từ Redis Cache.
/// 
/// Cách dùng:
///   [RequiresPermission("GET", "/api/v1/admin/accounts-page")]
///   public class AccountPageController : ControllerBase { ... }
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresPermissionAttribute : Attribute
{
    public string Method { get; }
    public string ApiPath { get; }

    public RequiresPermissionAttribute(string method, string apiPath)
    {
        Method = method.ToUpper();
        ApiPath = apiPath.ToLower();
    }
}
