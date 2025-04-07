using SyntheticArbitrage.Consumer.RabbitMQ;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Shared.RabbitMQModels;

var builder = WebApplication.CreateBuilder(args);
// Add config from external JSON
var sharedProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SyntheticArbitrage.Shared", "rabbitmqsettings.json");
builder.Configuration.AddJsonFile(sharedProjectPath, optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHttpClient<IBinanceHttpService, BinanceHttpService>();
builder.Services.AddHostedService<TickerPriceConsumerService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
