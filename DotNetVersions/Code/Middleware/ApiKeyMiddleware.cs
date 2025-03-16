namespace DotNetVersions.Code.Middleware;

public class ApiKeyMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeaderName = "x-api-key";

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        var configuredApiKey = configuration.GetValue<string>("ApiKey");
        if (string.IsNullOrWhiteSpace(configuredApiKey))
            throw new Exception("API Key is missing in configuration");

        if (configuredApiKey != extractedApiKey)
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await next(context);
    }
}
