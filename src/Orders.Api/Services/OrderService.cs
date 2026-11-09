using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<Order> GetOpenOrders(OrderFilterRequest filter)
    {
        var orders = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .ToList();

        var result = orders
            .Where(o => o.Status == "Open")
            .ToList();

        if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
        {
            result = result
                .Where(o => o.OrderNumber.Contains(filter.OrderNumber))
                .ToList();
        }

        if (filter.From.HasValue)
        {
            result = result.Where(o => o.CreatedAt >= filter.From.Value).ToList();
        }

        if (filter.To.HasValue)
        {
            result = result.Where(o => o.CreatedAt <= filter.To.Value).ToList();
        }

        return result;
    }

    public Order CompleteOrder(int orderId, CompleteOrderRequest request)
    {
        try
        {
            var order = _db.Orders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                .Result;

            if (order == null)
            {
                throw new InvalidOperationException($"Order {orderId} not found");
            }

            order.Status = "Completed";
            order.CompletedAt = DateTime.Now;

            _db.SaveChanges();

            _logger.LogInformation("Order {OrderId} completed. Comment: {Comment}", orderId, request.Comment);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete order {OrderId}", orderId);
            return null!;
        }
    }
}
