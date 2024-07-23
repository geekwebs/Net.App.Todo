using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Net.App.Consumer.Services;

public class WhatsAppConsumerService : DirectConsumerService 
{
    public WhatsAppConsumerService(IOptions<RabbitMqOptions> options) : base(options, "net.app.notifications", "net.app.notifications.whatsapp.queue", "whatsapp") { }

    protected override Task HandleMessageAsync(string message)
    {
        Console.WriteLine($" [x] WhatsApp Consumer Received: {message}");
        // Implement WhatsApp sending logic here
        return Task.CompletedTask;
    }
}