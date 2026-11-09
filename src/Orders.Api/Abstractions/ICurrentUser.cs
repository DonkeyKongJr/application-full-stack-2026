namespace Orders.Api.Abstractions;

public interface ICurrentUser
{
    int CustomerId { get; }
    string UserId { get; }
}
