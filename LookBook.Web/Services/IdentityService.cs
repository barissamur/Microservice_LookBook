using Newtonsoft.Json;
using System.Text;

namespace LookBook.Web.Services;

public class IdentityService
{
    private readonly HttpClient _client;

    public IdentityService(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("IdentityService");
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var loginUrl = "/login"; // Login endpoint'inin yolu
        var data = new { Username = username, Password = password };
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(loginUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            return token; // Token döndür
        }
        else
        {
            // İstek başarısız oldu, uygun bir hata yönetimi yapın
            throw new Exception("Login failed");
        }
    }
}
