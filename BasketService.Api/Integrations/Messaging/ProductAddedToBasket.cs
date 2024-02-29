namespace BasketService.Api.Integrations.Messaging;

public class ProductAddedToBasket
{
    public string BasketId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}
    