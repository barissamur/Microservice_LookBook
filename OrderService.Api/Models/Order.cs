namespace OrderService.Api.Models;

public class Order
{
    // Özellikler (Properties)
    public int OrderId { get; set; }
    public string CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; }
    public decimal OrderTotal { get; set; }
    public string PaymentMethod { get; set; }
    public string ShippingAddress { get; set; }
    public string TrackingCode { get; set; }

    public List<OrderItem> OrderItems { get; set; } = [];
    public List<PaymentDetail> PaymentDetails { get; set; } = [];
    public List<ShippingDetail> ShippingDetails { get; set; } = [];

    // Yapıcı Metod (Constructor)
    public Order(int orderId, string customerId, DateTime orderDate, string orderStatus,
                decimal orderTotal, string paymentMethod, string shippingAddress, string trackingCode)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OrderDate = orderDate;
        OrderStatus = orderStatus;
        OrderTotal = orderTotal;
        PaymentMethod = paymentMethod;
        ShippingAddress = shippingAddress;
        TrackingCode = trackingCode;
    }

    // Metodlar
    public void AddOrderDetails(string orderDetails)
    {
        // Sipariş detaylarını saklama veya işlemleri buraya yazabilirsiniz.
    }

    public void AddInvoiceUrl(string invoiceUrl)
    {
        // Fatura URL'sini saklama işlemini buraya yazabilirsiniz.
    }

    public void CancelOrder(string cancellationReason)
    {
        OrderStatus = "Cancelled";
        // İptal nedeninin kaydedilmesi gibi işlemleri buraya yazabilirsiniz.
    }
}