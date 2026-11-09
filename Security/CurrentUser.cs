namespace RaceDay.Api.Security;

public class CurrentUser(IHttpContextAccessor accessor)
{
    private HttpContext? Context => accessor.HttpContext;
    public int? Id => Context?.Session.GetInt32("UserId");
    public string? Role => Context?.Session.GetString("UserRole");
    public bool IsAuthenticated => Id.HasValue && !string.IsNullOrWhiteSpace(Role);
    public bool IsRole(string role) => IsAuthenticated && string.Equals(Role, role, StringComparison.Ordinal);
}
