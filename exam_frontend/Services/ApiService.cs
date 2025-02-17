using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend;

public class ApiService : IApiService
{
    private const string api_url = "https://localhost:7117/";
    private readonly HttpClient client;


    public ApiService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<T> GetAsync<T>(string url)
    {
        
            HttpResponseMessage response = await client.GetAsync(api_url + url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            } 
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
            }
        
        return default;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(api_url + url, data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        return default;
    }
}
