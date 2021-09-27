using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private bool _isExchangeCreated = false;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = "/",
                UserName = "guest",
                Password = "guest",
                Port = 5672,
            };

            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Publish message? (y/n)");
                string publishMessage = Console.ReadLine();

                if (publishMessage.ToLower() == "y")
                    PublishEvent(channel);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private void PublishEvent(IModel publishingChannel)
        {
            var dev = new Developer("Kevin", "Guedes", 18);
            var devCreatedEvent = new DeveloperCreatedEvent(dev);
            var message = JsonConvert.SerializeObject(devCreatedEvent);
            var body = Encoding.UTF8.GetBytes(message);

            if (!_isExchangeCreated)
            {
                publishingChannel.ExchangeDeclare(
                   exchange: "demo-x",
                   type: "direct",
                   durable: true,
                   autoDelete: false);

                _isExchangeCreated = true;
            }

            publishingChannel.BasicPublish(
                    exchange: "demo-x",
                    routingKey: typeof(DeveloperCreatedEvent).Name,
                    body: body);

            _logger.LogInformation("Message published");
        }
    }
}
