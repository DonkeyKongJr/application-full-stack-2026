using Orders.Api.Abstractions;
using Orders.Api.Models;

namespace Orders.Api.Infrastructure;

public sealed class LoggingOrderCompletedNotifier : IOrderCompletedNotifier
{
    private readonly ILogger<LoggingOrderCompletedNotifier> _logger;

    public LoggingOrderCompletedNotifier(ILogger<LoggingOrderCompletedNotifier> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(Order order, string? comment, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Order {OrderNumber} completed. Comment: {Comment}",
            order.OrderNumber,
            comment);

        return Task.CompletedTask;
    }
}
