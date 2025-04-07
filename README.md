# SyntheticArbitrage
Web API synthetic arbitrage for crypto pair

1) В проекте SyntheticArbitrage.DAL лежит docker-compose.yml и в нем надо запустить docker compose up -d 
Это создаст и запустит конйтенер с БД postgres.
Затем надо в Package Manager Console обновить БД, чтобы к ней прошла миграция PM> Update-Database

2)при помощи докера запустим RabbitMQ
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4.0-management

3)в appsettings.Development.json проекта DataCollectorWorker добавим настройку Serilog
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],

    "Properties": {
      "Application": "DataCollectorWorker"
    }
  }
}

4)в appsettings.Development.json проекта SyntheticArbitrage.API добавим настройку подключения к БД (Host=172.17.217.4 сюда надо адрес контейнера с БД) и настройку RabbitMQ
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=172.17.217.4;Database=arbitrage;Port=5432;Username=postgres;Password=123456"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}

5)создать rabbitmqsettings.json проекта SyntheticArbitrage.Shared, добавим настройку RabbitMQ который будет использовать Consumer
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}


6) запускаем главный проект SyntheticArbitrage.API
7) запускаем Consumer в SyntheticArbitrage.Consumer> dotnet run
8) запускаем worker, который будет обращаться к нашей апи и снимать показания цен в DataCollectorWorker> dotnet run

9) Скачиваем Prometheus, распаковываем, заменяем его prometheus.yml на наш, который лежит в SyntheticArbitrage.API/Prometheus/prometheus.yml.
Следует обратить внимание на то, на каком порту работает наша апишка и подставить необходимый в - targets: ['localhost:7071']
scrape_configs:
  - job_name: 'synthetic-arbitrage-api'
    scrape_interval: 10s
    metrics_path: /metrics
    scheme: https
    static_configs:
      - targets: ['localhost:7071']
        labels:
          app_name: 'SyntheticArbitrage.API'
    tls_config:
      insecure_skip_verify: true #отключает проверку TLS-сертификата (пригодится, если self-signed или dev-сертификат)

10)Запускаем Prometheus и переходим по адресу http://localhost:9090/ где в поле выражения вставляем 
{__name__=~"btcusdt_quarter_price|btcusdt_biquarter_price", app_name="SyntheticArbitrage.API"}
и смотрим отображение квартальных и биквартальных цен (пример в SyntheticArbitrage.API\Prometheus\RegularExpressionPrometheus.txt)

так же метрики можно увидеть в localhost:7071/metrics

11)С Grafana связаться не удалось, т.к. сервис недоступен. В проекте так же подключен swagger.