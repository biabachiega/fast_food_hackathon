using MenuApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<MenuDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
// Retry para aguardar o banco estar pronto
var maxRetries = 10;
var delay = TimeSpan.FromSeconds(2);
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        db.Database.Migrate();
        break;
    }
    catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P03") // database system is starting up
    {
        Console.WriteLine("Banco de dados está iniciando, aguardando...");
        Thread.Sleep(delay);
    }
    catch (Exception ex)
    {
        if (i == maxRetries - 1)
            throw;
        Console.WriteLine($"Erro ao conectar ao banco: {ex.Message}. Tentando novamente...");
        Thread.Sleep(delay);
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add metrics endpoint for Zabbix monitoring
app.MapGet("/metrics", () =>
{
    var process = Process.GetCurrentProcess();
    return Results.Json(new
    {
        timestamp = DateTime.UtcNow,
        service = "MenuAPI",
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
