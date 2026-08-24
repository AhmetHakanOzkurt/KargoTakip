using NotificationService.Messaging;
using KargoTakip.ServiceDefaults;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceDefaultsExtensions.KargoLoglamaKur();

            var builder = WebApplication.CreateBuilder(args);

            // Serilog, CORS, Swagger, DbContext, JWT ve authorization kurulumu ortak.
            builder.AddKargoServiceDefaults("NotificationService");

            builder.Services.AddSingleton<NotificationService.Services.EmailService>();

            // RabbitMQ kuyruklarini dinleyip bildirim ve e-posta uretir.
            builder.Services.AddHostedService<RabbitMqConsumer>();

            var app = builder.Build();

            app.UseKargoServiceDefaults();

            app.Run();
        }
    }
}
