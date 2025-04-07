using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;
using SyntheticArbitrage.API.Prometheus;
using SyntheticArbitrage.API.RabbitMQ;
using SyntheticArbitrage.DAL;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Shared.RabbitMQModels;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add config from external JSON
var sharedProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SyntheticArbitrage.Shared", "rabbitmqsettings.json");
builder.Configuration.AddJsonFile(sharedProjectPath, optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHttpClient<IBinanceHttpService, BinanceHttpService>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SyntheticArbitrage API",
        Version = "v1",
        Description = "API for calculating BTCUSDT quarters difference of prices.",
    });
    options.EnableAnnotations();
});

// Add dbContext
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BinanceDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add HealthCheck for Prometheus
builder.Services.AddHealthChecks();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Prometheus
//https://github.com/prometheus-net/prometheus-net#static-labels
Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
{
    {"app_name","SyntheticArbitrage.API"}
});
builder.Services.AddSingleton<IBtcUsdtPriceMetricsService,BtcUsdtPriceMetricsService>();

var app = builder.Build();

//for Prometheus
app.UseHttpMetrics();
app.MapMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint($"/swagger/v1/swagger.json", "SyntheticArbitrage");
        });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
