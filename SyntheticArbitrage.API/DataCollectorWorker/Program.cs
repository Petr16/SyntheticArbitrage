using DataCollectorWorker;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// ��������� Serilog ��� ����������� � �������
//Clear Providers 
builder.Logging.ClearProviders();
//Read appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)//read from appsettings.json
    .CreateLogger();
// ������������ Serilog � DI ����������
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
// add the provider
builder.Logging.AddSerilog();

builder.Services.AddHttpClient<SyntheticArbitrageWorker>();
builder.Services.AddHostedService<SyntheticArbitrageWorker>();

var host = builder.Build();
host.Run();
