using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddHttpClient<MenuService>(client =>
{
    client.BaseAddress = new Uri("http://menuapi-service:8080/");
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
db.Database.Migrate();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapGet("/metrics", () =>
{
    var process = Process.GetCurrentProcess();
    return Results.Json(new
    {
        timestamp = DateTime.UtcNow,
        service = "OrderAPI",
        metrics = new
        {
            memory_usage_mb = process.WorkingSet64 / 1024 / 1024,
            cpu_time_ms = process.TotalProcessorTime.TotalMilliseconds,
            thread_count = process.Threads.Count,
            gc_memory_mb = GC.GetTotalMemory(false) / 1024 / 1024,
            uptime_seconds = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime).TotalSeconds
        }
    });
});

app.Run();
