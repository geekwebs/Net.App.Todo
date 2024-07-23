using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Net.App.Consumer.Services;

public abstract class DirectConsumerService : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly string _routingKey;
    private IConnection _connection;
    private IModel _channel;

    protected DirectConsumerService(IOptions<RabbitMqOptions> options, string exchangeName, string queueName, string routingKey)
    {
        _options = options.Value;
        _exchangeName = exchangeName;
        _queueName  = queueName;
        _routingKey = routingKey;

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_options.Uri),
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel    = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _options.NotificationExchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await HandleMessageAsync(message);
            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    protected abstract Task HandleMessageAsync(string message);

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}