namespace exam_frontend.Middlewares;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);
        
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            context.Response.Redirect("/Account/Login");
        }
    }
}