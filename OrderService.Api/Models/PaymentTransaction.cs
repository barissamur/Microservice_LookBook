namespace OrderService.Api.Models;

public class PaymentTransaction
{
    public int PaymentTransactionId { get; set; }
    public int OrderId { get; set; }
    public string PaymentGateway { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
}
