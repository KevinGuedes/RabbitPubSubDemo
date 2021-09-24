using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
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
                Console.WriteLine("Publicar mensagem? (s/n)");
                string publishMessage = Console.ReadLine();

                if (publishMessage.ToLower() == "s")
                    PublishEvent(channel);
            }
        }

        private void PublishEvent(IModel publishingChannel)
        {
            if (!_isExchangeCreated)
            {
                publishingChannel.ExchangeDeclare(
                   exchange: "demo-x",
                   type: "direct",
                   durable: true,
                   autoDelete: false);

                _isExchangeCreated = true;
            }

            var message = "Olá, time!";
            var body = Encoding.UTF8.GetBytes(message);

            publishingChannel.BasicPublish(
                    exchange: "demo-x",
                    routingKey: "demo-message",
                    body: body);

            _logger.LogInformation("Message published");
        }
    }
}
