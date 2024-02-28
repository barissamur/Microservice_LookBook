using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Domain.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BasketService.Api.Infrastructure.Repository;

public class RedisBasketRepository : IBasketRepository
{
    private readonly ILogger<RedisBasketRepository> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisBasketRepository(ILoggerFactory loggerFactory, IConnectionMultiplexer redis)
    {
        _logger = loggerFactory.CreateLogger<RedisBasketRepository>();
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<bool> DeleteBasketAsync(string id)
    {
        return await _database.KeyDeleteAsync(id);
    }

    public IEnumerable<string> GetUsers()
    {
        var server = GetServer();
        var data = server.Keys();

        return data?.Select(k => k.ToString());
    }

    public async Task<CustomerBasket> GetBasketAsync(string customerId)
    {
        var data = await _database.StringGetAsync(customerId);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<CustomerBasket>(data);
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        var created = await _database.StringSetAsync(basket.BuyerId, JsonConvert.SerializeObject(basket));

        if (!created)
        {
            _logger.LogInformation("Problem occur persisting the item.");
            return null;
        }

        _logger.LogInformation("Basket item persisted succesfully.");

        return await GetBasketAsync(basket.BuyerId);
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints();
        return _redis.GetServer(endpoint.First());
    }

    public async Task<CustomerBasket> AddItemToBasketAsync(string customerId, BasketItem item)
    {
        // Müşterinin mevcut sepetini al
        var basket = await GetBasketAsync(customerId);
        if (basket == null)
        {
            basket = new CustomerBasket { BuyerId = customerId, Items = new List<BasketItem>() };
        }

        // Yeni ürünü sepete ekle
        basket.Items.Add(item);

        // Sepeti güncelle
        await UpdateBasketAsync(basket);

        return basket;
    }
}
