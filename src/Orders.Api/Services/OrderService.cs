using Billing.Data;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Abstractions;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly BillingDbContext _billing;
    private readonly ICurrentUser _currentUser;
    private readonly IOrderCompletedNotifier _notifier;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        AppDbContext db,
        BillingDbContext billing,
        ICurrentUser currentUser,
        IOrderCompletedNotifier notifier,
        ILogger<OrderService> logger)
    {
        _db = db;
        _billing = billing;
        _currentUser = currentUser;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<PagedResult<OrderListItemDto>> GetOpenOrdersAsync(
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 100 ? 20 : filter.PageSize;

        // Query filter already restricts to current customer + not deleted
        var query = _db.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Open);

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = _db.Orders
                .FromSqlRaw($"SELECT * FROM Orders ORDER BY {filter.SortBy}")
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Open);
        }

        if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
        {
            query = query.Where(o => o.OrderNumber.Contains(filter.OrderNumber));
        }

        if (filter.From.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= filter.To.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .Include(o => o.Customer)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = orders.Select(MapToListItem).ToList();

        return new PagedResult<OrderListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<bool> CompleteAsync(
        int orderId,
        CompleteOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: allow completing orders that were soft-deleted by mistake
        // and later restored in admin tools (IsDeleted cleared asynchronously).
        var order = await _db.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(
                o => o.Id == orderId && !o.IsDeleted,
                cancellationToken);

        if (order is null || order.Status != OrderStatus.Open)
        {
            return false;
        }

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        var account = await _billing.BillingAccounts
            .FirstOrDefaultAsync(a => a.CustomerId == order.CustomerId, cancellationToken);

        if (account is not null)
        {
            account.OpenBalance -= order.TotalAmount;
            account.LastActivityAt = DateTime.UtcNow;
        }

        await _notifier.NotifyAsync(order, request.Comment, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await _billing.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} completed by user {UserId}",
            orderId,
            _currentUser.UserId);

        return true;
    }

    private static OrderListItemDto MapToListItem(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerName = order.Customer.Name,
        CreatedAt = order.CreatedAt,
        TotalAmount = order.TotalAmount,
        LineCount = order.Lines.Count,
    };
}
