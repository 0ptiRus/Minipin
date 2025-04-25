namespace exam_frontend;

public class RedirectUnauthorizedMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectUnauthorizedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Capture the original response stream
        var originalBodyStream = context.Response.Body;

        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
        {
            context.Response.Body = originalBodyStream;

            // Redirect to login page
            context.Response.Redirect("/Account/Login"); // Adjust to your actual login page route
        }
        else
        {
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBodyStream);
        }
    }
}
