using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Net.App.Consumer.Services;

public class EmailConsumerService : DirectConsumerService
{
    public EmailConsumerService(IOptions<RabbitMqOptions> options) : base(options, "net.app.notifications", "net.app.notifications.email.queue", "email") { }

    protected override Task HandleMessageAsync(string message)
    {
        Console.WriteLine($" [x] Email Consumer Received: {message}");
        // Implement email sending logic here
        return Task.CompletedTask;
    }
}