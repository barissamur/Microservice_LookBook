using Newtonsoft.Json;
using System.Text;

namespace LookBook.Web.Services;

public class IdentityService
{
    private readonly HttpClient _client;

    public IdentityService(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("BaseUrl");
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var loginUrl = "/v1/Account/login"; // Login endpoint'inin yolu
        var fullUrl = $"{_client.BaseAddress}{loginUrl}"; // Tam URL'yi oluştur
        var data = new { Username = username, Password = password };
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        Console.WriteLine($"Request URL: {fullUrl}"); // İstek atılan URL'yi konsola yazdır

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
