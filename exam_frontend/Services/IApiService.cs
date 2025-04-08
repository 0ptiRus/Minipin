namespace exam_frontend.Services;

public interface IApiService
{
    Task<HttpResponseMessage> GetAsync(string url);
    Task<T> GetWithContentAsync<T>(string url);
    T JsonToContent<T>(string json);
    Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
    Task<HttpResponseMessage> PostAsJsonAsync(string url, object data);
}