using ConsolidationService.BackgroundServices;
using ConsolidationService.Services;
using KargoTakip.ServiceDefaults;

namespace ConsolidationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceDefaultsExtensions.KargoLoglamaKur();

            var builder = WebApplication.CreateBuilder(args);

            // Serilog, CORS, Swagger, DbContext, JWT ve authorization kurulumu ortak.
            builder.AddKargoServiceDefaults("ConsolidationService");

            builder.Services.AddScoped<ConsolidationEngine>();

            // Belirli araliklarla konsolidasyon algoritmasini calistirir.
            builder.Services.AddHostedService<ConsolidationBackgroundService>();

            var app = builder.Build();

            app.UseKargoServiceDefaults();

            app.Run();
        }
    }
}
