using System.Diagnostics;
using System.Text.Json;

namespace IdentityService.Middleware;

public class RequestMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RequestMetricsMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await _next(context);
        
        stopwatch.Stop();

        _ = Task.Run(async () =>
        {
            try
            {
                await ReportMetricAsync(context, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reporting metrics: {ex.Message}");
            }
        });
    }

    private async Task ReportMetricAsync(HttpContext context, long responseTimeMs)
    {
        var monitoringApiUrl = _configuration["MonitoringApi:BaseUrl"] ?? "http://monitoringapi-service:8080";
        
        var metric = new
        {
            ServiceName = "identityapi",
            Method = context.Request.Method,
            Path = context.Request.Path.Value ?? "/",
            StatusCode = context.Response.StatusCode,
            ResponseTimeMs = responseTimeMs,
            Timestamp = DateTimeOffset.UtcNow
        };

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        var json = JsonSerializer.Serialize(metric);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            await httpClient.PostAsync($"{monitoringApiUrl}/api/MetricsCollector/report", content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to report metric to MonitoringAPI: {ex.Message}");
        }
    }
}
