using Kafka3.Data;
using Kafka3.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // ������ ������������ �� appsettings.json
    .Enrich.FromLogContext() // ���������� ��������� �����
    .CreateLogger();

builder.Host.UseSerilog(); // ����������� Serilog ��� �����������

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<KafkaService>();

var app = builder.Build();

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Start Kafka service within a scope
using (var scope = app.Services.CreateScope())
{
    var kafkaService = scope.ServiceProvider.GetRequiredService<KafkaService>();
    await kafkaService.StartAsync();
}

app.Run();
