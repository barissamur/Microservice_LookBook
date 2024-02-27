using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        var baseServiceClient = _clientFactory.CreateClient("BaseAdres");

        var placeholderValues = responses[0].Items.TemplatePlaceholderNameAndValues();
        var orderIdValue = placeholderValues.FirstOrDefault(p => p.Name == "{orderId}")?.Value;
        int orderId = Convert.ToInt32(orderIdValue);


        if (!string.IsNullOrEmpty(token))
            baseServiceClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);

        // graphql Sorgusu tek satırda olacak
        var getOrderWithItemsGraphqlQuery = @"
        {
            ""query"": ""query { orderWithItems(orderId: " + orderId + @") { orderId customerId orderDate orderStatus orderItems { productId quantity unitPrice } } }""
        }";

        var orderContent = new StringContent(getOrderWithItemsGraphqlQuery, Encoding.UTF8, "application/json");

        var orderResponse = await baseServiceClient.PostAsync("Order/graphql", orderContent);

        var orderResponseContent = await orderResponse.Content.ReadAsStringAsync();

          
        var jObject = JObject.Parse(orderResponseContent);

        // `productId` değerlerini alın
        var productIds = jObject["data"]["orderWithItems"]["orderItems"]
                            .Select(item => (string)item["productId"])
                            .ToArray();
         
        var getBooksGraphqlQuery = @"
            query booksByIds($ids: [String!]!) { 
                booksByIds(ids: $ids) { 
                    id 
                    title 
                    author 
                    year 
                    price 
                } 
            }
        ";
         
        var requestBody = new
        {
            query = getBooksGraphqlQuery,
            variables = new { ids = productIds }
        };


        var bookContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
         
        var bookResponse = await baseServiceClient.PostAsync("Book/graphql", bookContent);

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
