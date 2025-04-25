using System.Net.Http.Headers;
using System.Text.Json;

namespace exam_frontend.Services;

public class ApiService : IApiService
{
    private string api_url;
    private readonly HttpClient client;
    private readonly IHttpContextAccessor accessor;

    public ApiService(HttpClient client, IHttpContextAccessor accessor, IConfiguration configuration)
    {
        this.client = client;   
        this.accessor = accessor;
        api_url = configuration["ApiSettings:BaseUrl"] 
                  ?? throw new InvalidOperationException("API Base URL is not configured.");
    }

    public void AppendToken()
    {
        string jwt_token = accessor.HttpContext?.Request.Cookies["jwt"];
        if (jwt_token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt_token);
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        try
        {
            AppendToken();
            return await client.GetAsync($"{api_url}/{url}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request was canceled: {ex.Message}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request error: {ex.Message}");
            throw;
        }
    }

    public async Task<T> GetWithContentAsync<T>(string url)
    {
        AppendToken();
        HttpResponseMessage response = await client.GetAsync($"{api_url}/{url}");
        T content = JsonToContent<T>(await response.Content.ReadAsStringAsync());
        return content;
    }

    public T JsonToContent<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        try
        {
            AppendToken();
            HttpResponseMessage response = await client.PostAsync($"{api_url}/{url}", content);
            return response;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request was canceled: {ex.Message}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request error: {ex.Message}");
            throw;
        }
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync(string url, object data)
    {
        try
        {
            AppendToken();
            HttpResponseMessage response = await client.PostAsJsonAsync($"{api_url}/{url}", data);
            return response;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request was canceled: {ex.Message}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request error: {ex.Message}");
            throw;
        }
    }
}