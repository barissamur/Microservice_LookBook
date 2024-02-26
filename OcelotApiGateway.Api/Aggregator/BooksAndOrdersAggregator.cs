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
        var booksResponse = await responses[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
        var ordersResponse = await responses[1].Items.DownstreamResponse().Content.ReadAsStringAsync();

        // Yanıtları JSON olarak parse etmek.
        var booksData = JsonConvert.DeserializeObject(booksResponse);
        var ordersData = JsonConvert.DeserializeObject(ordersResponse);

        // İki servisten gelen verileri birleştir.
        var combinedResult = new
        {
            Books = booksData,
            Orders = ordersData
        };

        // Birleştirilmiş veriyi JSON string'e çevir.
        var combinedJson = JsonConvert.SerializeObject(combinedResult);
        var contentBytes = Encoding.UTF8.GetBytes(combinedJson);

        // Birleştirilmiş veriyi bir MemoryStream'e yaz.
        var streamContent = new MemoryStream(contentBytes);
        var httpContent = new StreamContent(streamContent);

        // Gerekli başlıkları ayarla.
        var headers = new List<Header> { new Header("Content-Type", new[] { "application/json" }) };

        // DownstreamResponse nesnesini oluştur.
        var downstreamResponse = new DownstreamResponse(httpContent, HttpStatusCode.OK, headers, "OK");

        return downstreamResponse;
    }
}
