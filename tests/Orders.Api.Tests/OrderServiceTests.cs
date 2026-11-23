using Billing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Orders.Api.Abstractions;
using Orders.Api.Data;
using Orders.Api.Models;
using Orders.Api.Services;
using Xunit;

namespace Orders.Api.Tests;

public class OrderServiceTests
{
    private sealed class TestUser(int customerId = 1) : ICurrentUser
    {
        public int CustomerId { get; } = customerId;
        public string UserId => "user-1";
    }

    private sealed class RecordingNotifier : IOrderCompletedNotifier
    {
        public int Count { get; private set; }
        public Task NotifyAsync(Order order, string? comment, CancellationToken cancellationToken = default)
        {
            Count++;
            return Task.CompletedTask;
        }
    }

    private static AppDbContext CreateDb(TestUser user, string name) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options, user);

    private static BillingDbContext CreateBillingDb() =>
        new(new DbContextOptionsBuilder<BillingDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task SeedAsync(AppDbContext db)
    {
        db.Customers.AddRange(
            new Customer { Id = 1, Name = "Müller GmbH", Email = "a@ex.com" },
            new Customer { Id = 2, Name = "Schmidt AG", Email = "b@ex.com" });

        db.Orders.AddRange(
            new Order
            {
                Id = 10, OrderNumber = "A-100", CustomerId = 1, Status = OrderStatus.Open,
                CreatedAt = new DateTime(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc), TotalAmount = 1200m,
                Lines = { new OrderLine { Id = 1, ProductName = "Sensor", Quantity = 2, UnitPrice = 600m } },
            },
            new Order
            {
                Id = 11, OrderNumber = "A-101", CustomerId = 1, Status = OrderStatus.Open,
                CreatedAt = new DateTime(2026, 11, 9, 12, 30, 0, DateTimeKind.Utc), TotalAmount = 800m,
            },
            new Order
            {
                Id = 20, OrderNumber = "B-200", CustomerId = 2, Status = OrderStatus.Open,
                CreatedAt = new DateTime(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc), TotalAmount = 500m,
            });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetOpenOrders_FiltersByOrderNumber_ForCurrentCustomer()
    {
        var user = new TestUser(1);
        await using var db = CreateDb(user, Guid.NewGuid().ToString());
        await SeedAsync(db);
        var sut = new OrderService(db, CreateBillingDb(), user, new RecordingNotifier(), NullLogger<OrderService>.Instance);

        var result = await sut.GetOpenOrdersAsync(new OrderFilterRequest { OrderNumber = "A-10" });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, i => Assert.StartsWith("A-10", i.OrderNumber));
    }

    [Fact]
    public async Task GetOpenOrders_RespectsInclusiveDateRange()
    {
        var user = new TestUser(1);
        await using var db = CreateDb(user, Guid.NewGuid().ToString());
        await SeedAsync(db);
        var sut = new OrderService(db, CreateBillingDb(), user, new RecordingNotifier(), NullLogger<OrderService>.Instance);

        // Noon seed data; end-of-day To always includes them
        var result = await sut.GetOpenOrdersAsync(new OrderFilterRequest
        {
            From = new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 11, 9, 23, 59, 59, DateTimeKind.Utc),
        });

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetOpenOrders_Pagination_ReturnsDistinctPages()
    {
        var user = new TestUser(1);
        await using var db = CreateDb(user, Guid.NewGuid().ToString());
        await SeedAsync(db);
        var sut = new OrderService(db, CreateBillingDb(), user, new RecordingNotifier(), NullLogger<OrderService>.Instance);

        var page1 = await sut.GetOpenOrdersAsync(new OrderFilterRequest { Page = 1, PageSize = 1 });
        var page2 = await sut.GetOpenOrdersAsync(new OrderFilterRequest { Page = 2, PageSize = 1 });

        Assert.Single(page1.Items);
        Assert.Single(page2.Items);
        Assert.NotEqual(page1.Items[0].Id, page2.Items[0].Id);
    }

    [Fact]
    public async Task Complete_SetsStatusAndSendsNotification()
    {
        var user = new TestUser(1);
        await using var db = CreateDb(user, Guid.NewGuid().ToString());
        await SeedAsync(db);
        var notifier = new RecordingNotifier();
        var sut = new OrderService(db, CreateBillingDb(), user, notifier, NullLogger<OrderService>.Instance);

        var ok = await sut.CompleteAsync(10, new CompleteOrderRequest { Comment = "passt" });

        Assert.True(ok);
        Assert.Equal(1, notifier.Count);

        var tracked = await db.Orders.IgnoreQueryFilters().SingleAsync(o => o.Id == 10);
        Assert.Equal(OrderStatus.Completed, tracked.Status);
        Assert.NotNull(tracked.CompletedAt);
    }
}
