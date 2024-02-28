﻿using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Controller;


[Route("api/[controller]")]
[ApiController]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;

    public BasketController(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    // Müşteri sepetini getir
    [HttpGet("{customerId}")]
    public async Task<ActionResult<CustomerBasket>> GetBasket(string customerId)
    {
        var basket = await _basketRepository.GetBasketAsync(customerId);
        if (basket == null)
            return NotFound();

        return Ok(basket);
    }

    // Sepeti güncelle
    [HttpPut]
    public async Task<ActionResult<CustomerBasket>> UpdateBasket([FromBody] CustomerBasket basket)
    {
        var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);
        return Ok(updatedBasket);
    }

    // Sepeti sil
    [HttpDelete("{customerId}")]
    public async Task<ActionResult<bool>> DeleteBasket(string customerId)
    {
        var result = await _basketRepository.DeleteBasketAsync(customerId);
        if (!result)
            return NotFound();

        return Ok(result);
    }

    // Tüm kullanıcıları getir
    [HttpGet("users")]
    public ActionResult<IEnumerable<string>> GetUsers()
    {
        var users = _basketRepository.GetUsers();
        return Ok(users);
    }

    // Sepete ürün ekle
    [HttpPost("add/{customerId}")]
    public async Task<ActionResult<CustomerBasket>> AddItemToBasket(string customerId, [FromBody] BasketItem item)
    {
        var basket = await _basketRepository.AddItemToBasketAsync(customerId, item);
        if (basket == null)
            return NotFound("Basket not found or item could not be added");

        return Ok(basket);
    }
}