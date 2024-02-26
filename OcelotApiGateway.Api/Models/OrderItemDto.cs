namespace OrderService.Api.Models;

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; } // Siparişe referans
    public string ProductId { get; set; }
    public int Quantity { get; set; } // Sipariş edilen miktar
    public decimal UnitPrice { get; set; } // Ürünün birim fiyatı, Ürün servisinden gelir

    // Navigation property
    public OrderDto Order { get; set; }
}

