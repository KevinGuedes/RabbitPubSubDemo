using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private bool _isExchangeCreated = false;
        private bool _isQueueConfigured = false;

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
            StartConsuming<DeveloperCreatedEvent>(channel);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void StartConsuming<T>(IModel consumerChannel)
        {
            if (!_isExchangeCreated)
            {
                consumerChannel.ExchangeDeclare(
                   exchange: "demo-x",
                   type: "direct",
                   durable: true,
                   autoDelete: false);

                _isExchangeCreated = true;
            }

            if (!_isQueueConfigured)
            {
                consumerChannel.QueueDeclare(
                    queue: "demo",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                consumerChannel.QueueBind(
                        queue: "demo",
                        exchange: "demo-x",
                        routingKey: typeof(T).Name);

                _isQueueConfigured = true;
            }

            var consumer = new EventingBasicConsumer(consumerChannel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine("Event received");
                Console.WriteLine(message);
            };

            consumerChannel.BasicConsume(queue: "demo",
                               autoAck: true,
                               consumer: consumer);
        }
    }
}
