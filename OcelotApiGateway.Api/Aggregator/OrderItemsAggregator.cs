using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Net;
using System.Text;

namespace BookService.Api.Aggregator;

public class OrderItemsAggregator : IDefinedAggregator
{
    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.Append("{");

        // Yanıtların birleştirilmesi
        // Önceki örnekteki gibi yanıtları birleştirme işlemleri burada yapılabilir.

        contentBuilder.Append("}");
        var contentString = contentBuilder.ToString();
        var byteArrayContent = Encoding.UTF8.GetBytes(contentString);

        // MemoryStream üzerinden HttpContent oluşturun
        var streamContent = new MemoryStream(byteArrayContent);
        var httpContent = new StreamContent(streamContent);

        // DownstreamResponse için gerekli header'ları hazırlayın
        var headers = new List<Header> { new Header("Content-Type", new[] { "application/json" }) };

        // DownstreamResponse nesnesini oluşturun
        var downstreamResponse = new DownstreamResponse(httpContent, HttpStatusCode.OK, headers, "");

        return downstreamResponse;
    }

}
