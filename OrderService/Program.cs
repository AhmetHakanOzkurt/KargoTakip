using FluentValidation;
using KargoTakip.ServiceDefaults;

ServiceDefaultsExtensions.KargoLoglamaKur();

var builder = WebApplication.CreateBuilder(args);

// Serilog, CORS, Swagger, DbContext, JWT ve authorization kurulumu ortak.
builder.AddKargoServiceDefaults("OrderService");

builder.Services.AddValidatorsFromAssemblyContaining<OrderService.Validators.CreateShipmentRequestValidator>();

// VehicleService cagrisi: IHttpClientFactory ile yonetilir, boylece her
// istekte yeni HttpClient olusturulmaz.
builder.Services.AddHttpClient("vehicle-service", client =>
{
    // Sertifika dogrulamasini devre disi birakan handler kaldirildi.
    // Docker'da bu adres http://vehicle-service:8080 olarak verilir;
    // yerelde VehicleService'in HTTP portu kullanilir. Boylece self-signed
    // sertifika sorunu hic olusmaz ve TLS dogrulamasi her ortamda acik kalir.
    var baseUrl = builder.Configuration["VehicleService:BaseUrl"]
        ?? "http://localhost:5193";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Kalici RabbitMQ baglantisi tutar; singleton olmasi zorunludur.
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var hostName = config["RabbitMQ:HostName"] ?? "localhost";
    var logger = sp.GetRequiredService<ILogger<OrderService.Messaging.RabbitMqProducer>>();
    return new OrderService.Messaging.RabbitMqProducer(hostName, logger);
});

// Outbox tablosundaki event'leri kuyruga aktarir.
builder.Services.AddHostedService<OrderService.Messaging.OutboxYayinlayici>();

var app = builder.Build();

app.UseKargoServiceDefaults();

app.Run();
