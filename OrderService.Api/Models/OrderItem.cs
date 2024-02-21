namespace OrderService.Api.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; } // Siparişe referans
    public string? ProductId { get; set; }
    public int Quantity { get; set; } // Sipariş edilen miktar
    public decimal UnitPrice { get; set; } // Ürünün birim fiyatı, Ürün servisinden gelir

    // Navigation property
    public Order Order { get; set; }
}

