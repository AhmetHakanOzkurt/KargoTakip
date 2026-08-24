using KargoTakip.ServiceDefaults;

namespace ReportService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceDefaultsExtensions.KargoLoglamaKur();

            var builder = WebApplication.CreateBuilder(args);

            // Serilog, CORS, Swagger, DbContext, JWT ve authorization kurulumu ortak.
            builder.AddKargoServiceDefaults("ReportService");

            var app = builder.Build();

            app.UseKargoServiceDefaults();

            app.Run();
        }
    }
}
