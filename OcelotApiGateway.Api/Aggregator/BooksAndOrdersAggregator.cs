using Newtonsoft.Json;
using Ocelot.Infrastructure.Extensions;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BookService.Api.Aggregator;

public class BooksAndOrdersAggregator : IDefinedAggregator
{
    private readonly IHttpClientFactory _clientFactory;

    public BooksAndOrdersAggregator(IHttpClientFactory httpClientFactory)
    {
        _clientFactory = httpClientFactory;
    }

    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var token = responses.FirstOrDefault()?.Request.Headers["Authorization"].GetValue();

        var placeholderValues = responses[0].Items.TemplatePlaceholderNameAndValues();
        var orderIdValue = placeholderValues.FirstOrDefault(p => p.Name == "{orderId}")?.Value;
        int orderId = Convert.ToInt32(orderIdValue);

        var orderServiceClient = _clientFactory.CreateClient("OrderServiceGraphQLClient");

        if (!string.IsNullOrEmpty(token))
            orderServiceClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);

        // graphql Sorgusu tek satırda olacak
        var getOrderWithItemsGraphqlQuery = @"
        {
            ""query"": ""query { orderWithItems(orderId: " + orderId + @") { orderId customerId orderDate orderStatus orderItems { productId quantity unitPrice } } }""
        }";

        var orderContent = new StringContent(getOrderWithItemsGraphqlQuery, Encoding.UTF8, "application/json");

        var orderResponse = await orderServiceClient.PostAsync("Order/graphql", orderContent);

        var orderResponseContent = await orderResponse.Content.ReadAsStringAsync();



        // burada order içeriğinden product id'leri çekecez




        // bookId'lere göre book servise istek atacaz
        var bookServiceClient = _clientFactory.CreateClient("BookServiceGraphQLClient");

        if (!string.IsNullOrEmpty(token))
            bookServiceClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);
         
        // GraphQL sorgusu JSON formatında hazırlanır
        string getBooksGraphqlQuery = @"
        {
            ""query"": ""query { booksByIds(ids: [" + 1 + @"]) { id title author year price }}""
        }";

        var bookContent = new StringContent(getBooksGraphqlQuery, Encoding.UTF8, "application/json");

        var bookResponse = await bookServiceClient.PostAsync("Book/graphql", bookContent);

        var bookResponseContent = await bookResponse.Content.ReadAsStringAsync();

        var headers = new List<Header>();

        if (bookResponse.IsSuccessStatusCode)
        {
            var responseString = await bookResponse.Content.ReadAsStringAsync();
            headers.Add(new Header("Content-Type", new[] { "application/json" }));


            // Başarılı bir yanıt döndürülür
            return new DownstreamResponse(new StringContent(responseString, Encoding.UTF8, "application/json"), System.Net.HttpStatusCode.OK, headers, "OK");
        }

        else
        {
            // Hata durumunda uygun bir DownstreamResponse döndürülür
            return new DownstreamResponse(null, System.Net.HttpStatusCode.BadRequest, headers, null);
        }
    }
}
