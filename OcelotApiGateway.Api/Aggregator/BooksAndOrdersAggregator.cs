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

        //// Sorgu metnini JSON string olarak düzenliyoruz
        //string getOrderWithItemsQuery = @"
        //{
        //    ""query"": ""query {
        //        orderWithItems(orderId: " + orderIdValue + @") {
        //            orderId
        //            customerId
        //            orderDate
        //            orderStatus
        //            orderItems {
        //                productId
        //                quantity
        //                unitPrice
        //            }
        //        }
        //    }""
        //}";

        var getOrderWithItemsQuery = @"
        {
            ""query"": ""query { 
                orderWithItems(orderId: " + orderId + @") { 
                    orderId 
                    customerId 
                    orderDate 
                    orderStatus 
                    orderItems { 
                        productId 
                        quantity 
                        unitPrice 
                    } 
                } 
            }""
        }";

        var orderContent = new StringContent(getOrderWithItemsQuery, Encoding.UTF8, "application/json");

        var orderResponse = await orderServiceClient.PostAsync("Order/graphql", orderContent);

        var orderResponseContent = await orderResponse.Content.ReadAsStringAsync();




        var bookServiceClient = _clientFactory.CreateClient("BookServiceGraphQLClient");

        if (!string.IsNullOrEmpty(token))
            bookServiceClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);


        // GraphQL sorgusu JSON formatında hazırlanır
        string graphqlQuery = @"
        {
            ""query"": ""{ 
                booksByIds(ids: [\""65d48c5bb7580d61bec6e3fd\"", \""65d5dbb8542de6db8f1f91af\""]) { 
                    id 
                    title 
                    author 
                    year 
                    price 
                } 
              }""        }";

        // StringContent nesnesi oluşturulurken, GraphQL sorgusu JSON string olarak verilir
        var content = new StringContent(graphqlQuery, Encoding.UTF8, "application/json");

        // HttpClient ile POST isteği yapılır
        var response = await bookServiceClient.PostAsync("/Book/graphql", content);

        // Yanıtın içeriği okunur
        var responseContent = await response.Content.ReadAsStringAsync();

        var headers = new List<Header>();

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
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
