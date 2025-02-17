using exam_frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace exam_frontend;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(UserLoginModel loginData);
    Task<RegisterResponse> RegisterAsync(UserRegisterModel registerData);
}

public class AuthService : IAuthService
{
    private const string api_url = "https://localhost:7117/";
    private readonly IApiService api_service;

    public AuthService(IApiService apiService)
    {
        api_service = apiService;
    }

    public async Task<LoginResponse> LoginAsync(UserLoginModel loginData)
    {
        return await api_service.PostAsync<UserLoginModel, LoginResponse>("api/account/login", loginData);
    }

    public async Task<RegisterResponse> RegisterAsync(UserRegisterModel registerData)
    {
        RegisterResponse result = await api_service.PostAsync<UserRegisterModel, RegisterResponse>("api/account/register", registerData);
        return result;
    }
}

public class LoginResponse
{
    public string Message { get; set; }
}

public class RegisterResponse
{
    public bool IsCreated { get; set; }
    public string Message { get; set; }
}
