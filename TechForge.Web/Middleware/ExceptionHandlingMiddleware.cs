namespace TechForge.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // In development, re-throw so the detailed developer page is shown.
            if (_env.IsDevelopment())
            {
                throw;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.Redirect("/Error/500");
            }
        }
    }
}
