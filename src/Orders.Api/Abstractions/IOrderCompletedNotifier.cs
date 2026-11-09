using Orders.Api.Models;

namespace Orders.Api.Abstractions;

public interface IOrderCompletedNotifier
{
    Task NotifyAsync(Order order, string? comment, CancellationToken cancellationToken = default);
}
