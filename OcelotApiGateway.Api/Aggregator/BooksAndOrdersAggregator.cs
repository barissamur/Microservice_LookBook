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
        var placeholderValues = responses[0].Items.TemplatePlaceholderNameAndValues();

        var orderIdValue = placeholderValues.FirstOrDefault(p => p.Name == "{orderId}")?.Value;

        var client = _clientFactory.CreateClient("BookServiceGraphQLClient");
        var token = responses.FirstOrDefault()?.Request.Headers["Authorization"].GetValue();

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);


        // GraphQL sorgusu JSON formatında hazırlanır
        string graphqlQuery = @"
        {
            ""query"": ""{ booksByIds(ids: [\""65d383357e097752097694bc\"", \""65dcbf748d7f32dca48ed82c\""]) { id title author year price } }""
        }";

        // StringContent nesnesi oluşturulurken, GraphQL sorgusu JSON string olarak verilir
        var content = new StringContent(graphqlQuery, Encoding.UTF8, "application/json");

        // HttpClient ile POST isteği yapılır
        var response = await client.PostAsync("", content);

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
