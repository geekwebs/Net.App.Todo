
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Net.App.Consumer.Services;
using Net.App.Security.Crypto;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    //   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<RabbitMqOptions>(hostContext.Configuration.GetSection("RabbitMQ"));
                services.AddSingleton(sp => 
                {
                    var Key = hostContext.Configuration["Encryption:Key"];
                    var Iv = hostContext.Configuration["Encryption:IV"];
                    return new AesEncryptor(Key, Iv);
                });
                services.AddHostedService<EmailConsumerService>();
                services.AddHostedService<WhatsAppConsumerService>();
            });

}