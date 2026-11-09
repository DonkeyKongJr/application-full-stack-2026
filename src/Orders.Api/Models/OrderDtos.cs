namespace Orders.Api.Models;

public class OrderFilterRequest
{
    public string? OrderNumber { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class CompleteOrderRequest
{
    public string? Comment { get; set; }
}
