using BasketService.Api.Core.Domain.Models;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace BasketService.Api.Core.Application.Repository;

public interface IBasketRepository
{
    Task<CustomerBasket> GetBasketAsync(string customerId);

    IEnumerable<string> GetUsers();
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);

    Task<bool> DeleteBasketAsync(string id);

    Task<CustomerBasket> AddItemToBasketAsync(string customerId, BasketItem item);
}
