namespace OrderService.Api.Models;

public class PaymentDetail
{
    public int PaymentDetailId { get; set; }
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } // Ödeme yöntemi
    public DateTime PaymentDate { get; set; } // Ödeme tarihi
    public decimal AmountPaid { get; set; } // Ödenen miktar
    public string Status { get; set; } // Ödeme durumu

    // Navigation property
    public Order Order { get; set; }
}
