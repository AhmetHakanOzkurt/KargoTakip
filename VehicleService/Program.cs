using KargoTakip.ServiceDefaults;

namespace VehicleService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceDefaultsExtensions.KargoLoglamaKur();

            var builder = WebApplication.CreateBuilder(args);

            // Serilog, CORS, Swagger, DbContext, JWT ve authorization kurulumu ortak.
            builder.AddKargoServiceDefaults("VehicleService");

            var app = builder.Build();

            app.UseKargoServiceDefaults();

            app.Run();
        }
    }
}
