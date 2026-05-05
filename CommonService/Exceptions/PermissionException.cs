namespace CommonService.Exceptions;

public class PermissionException : Exception
{
    public PermissionException(string message) : base(message)
    {
    }
}
