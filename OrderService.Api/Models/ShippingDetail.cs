namespace OrderService.Api.Models;

public class ShippingDetail
{
    public int ShippingDetailId { get; set; }
    public int OrderId { get; set; }
    public string ShippingAddress { get; set; } // Kargo adresi
    public string TrackingCode { get; set; } // Kargo takip kodu
    public string Status { get; set; } // Kargo durumu

    // Navigation property
    public Order Order { get; set; }
}
