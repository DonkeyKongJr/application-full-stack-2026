using Orders.Api.Abstractions;

namespace Orders.Api.Infrastructure;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int CustomerId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst("customer_id")?.Value;
            return int.TryParse(value, out var customerId) ? customerId : 0;
        }
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value ?? string.Empty;
}
