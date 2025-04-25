using exam_frontend.Models;

namespace exam_frontend.Services;


public interface IAuthService
{
    Task<LoginResponse> LoginAsync(UserLoginModel loginData);
    Task<RegisterResponse> RegisterAsync(UserRegisterModel registerData);
}
 
public class AuthService : IAuthService
{
    public readonly IApiService api_service;
 
    public AuthService(IApiService apiService)
    {
        api_service = apiService;
    }
 
    public async Task<LoginResponse> LoginAsync(UserLoginModel loginData)
    {
        HttpResponseMessage response = await api_service.PostAsJsonAsync("Account/login", loginData);
        return api_service.JsonToContent<LoginResponse>(await response.Content.ReadAsStringAsync());
    }
 
    public async Task<RegisterResponse> RegisterAsync(UserRegisterModel registerData)
    {
        var content = new MultipartFormDataContent();

        content.Add(new StringContent(registerData.Username), "Username");
        content.Add(new StringContent(registerData.Email), "Email");
        content.Add(new StringContent(registerData.Password), "Password");

        if (registerData.Pfp != null)
        {
            var stream = registerData.Pfp.OpenReadStream(); 
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "Pfp", registerData.Pfp.FileName);
        }
        
        HttpResponseMessage response = await api_service.PostAsync("Account/register", content);
        return api_service.JsonToContent<RegisterResponse>(await response.Content.ReadAsStringAsync());
    }
}
 
public class LoginResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
}
 
public class RegisterResponse
{
    public bool IsCreated { get; set; }
    public string Message { get; set; }
}