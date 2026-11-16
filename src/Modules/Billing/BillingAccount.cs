namespace Billing.Models;

public class BillingAccount
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal OpenBalance { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime? LastActivityAt { get; set; }
}
