using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace exam_admin.Services;

public class ApiService : IApiService
{
    private readonly string api_url = "https://localhost:7279/api";
    private readonly HttpClient client;
    private readonly IHttpContextAccessor accessor;

    public ApiService(HttpClient client, IHttpContextAccessor accessor)
    {
        this.client = client;
        this.accessor = accessor;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, $"{api_url}/{url}");
        
        // Per-request token attachment
        var jwt_token = accessor.HttpContext?.Request.Cookies["jwt"];

        if (!string.IsNullOrEmpty(jwt_token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt_token);
        }

        if (content != null)
        {
            request.Content = content;
        }

        return request;
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        var request = CreateRequest(HttpMethod.Get, url);
        return await client.SendAsync(request);
    }

    public async Task<T> GetWithContentAsync<T>(string url)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, url);
        HttpResponseMessage response = await client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        return JsonToContent<T>(json);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Post, url, content);
        return await client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync(string url, object data)
    {
        string json = JsonSerializer.Serialize(data);
        StringContent content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpRequestMessage request = CreateRequest(HttpMethod.Post, url, content);
        return await client.SendAsync(request);
    }

    public T JsonToContent<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            })!;
    }
}
