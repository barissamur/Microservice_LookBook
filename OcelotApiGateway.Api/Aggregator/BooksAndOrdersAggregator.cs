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
        var client = _clientFactory.CreateClient("BookServiceGraphQLClient");
        var token = responses.FirstOrDefault()?.Request.Headers["Authorization"].GetValue();

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);


        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5000/v1/Book/graphql");

        // GraphQL sorgunuz
        string graphqlQuery = @"
        {
            ""query"": ""{ 
                booksByIds (ids: [\""65d48c5bb7580d61bec6e3fd\"", \""65d5dbee542de6db8f1f91b4\""]) { 
                    id 
                    title 
                    author 
                    year 
                    price 
                } 
            }""
        }";

        var content = new StringContent(graphqlQuery, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("", content);

        request.Content = new StringContent(JsonConvert.SerializeObject(graphqlQuery), Encoding.UTF8, "application/json");

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
