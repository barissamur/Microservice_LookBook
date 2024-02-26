using Newtonsoft.Json;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Net;
using System.Text;

namespace BookService.Api.Aggregator;

public class BooksAndOrdersAggregator : IDefinedAggregator
{
    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var placeholderValues = responses[0].Items.TemplatePlaceholderNameAndValues();

        var orderIdValue = placeholderValues.FirstOrDefault(p => p.Name == "{orderId}")?.Value;


        var bookServiceHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5010/graphql") // Book servisinin adresi
        };


        // Order servisine yönelik GraphQL sorgusu
        var bookQuery = @$" 
            {{
              booksByIds(ids: [""65d48c5bb7580d61bec6e3fd"", ""65d5dbee542de6db8f1f91b4""]) {{ 
                id
                title
                author
                year
                price
              }}
            }}";

        var bookResponseContent = new StringContent(JsonConvert.SerializeObject(new { query = bookQuery }), Encoding.UTF8, "application/json");
        var bookResponse = await bookServiceHttpClient.PostAsync("", bookResponseContent);
        var bookData = await bookResponse.Content.ReadAsStringAsync();


        var orderResponse = await responses[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
        //var bookResponse = await responses[1].Items.DownstreamResponse().Content.ReadAsStringAsync();

        // Verileri birleştirme işlemleri burada yapılır
        var combinedResult = $"{{\"order\": {orderResponse}, \"books\": {bookResponse}}}";

        var stringContent = new StringContent(combinedResult, System.Text.Encoding.UTF8, "application/json");

        var headers = new List<Header>();
        // Gerekli header'ları ekleyin

        return new DownstreamResponse(stringContent, HttpStatusCode.OK, headers, "OK");
    }
}
