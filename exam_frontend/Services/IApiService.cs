namespace exam_frontend;

public interface IApiService
{
    Task<T> GetAsync<T>(string url);
    Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data);
}
