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
    var baseUrl = builder.Configuration["VehicleService:BaseUrl"]
        ?? "https://localhost:7139";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // Self-signed localhost sertifikasi sadece gelistirmede kabul edilir.
    if (builder.Environment.IsDevelopment())
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

// Kalici RabbitMQ baglantisi tutar; singleton olmasi zorunludur.
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var hostName = config["RabbitMQ:HostName"] ?? "localhost";
    var logger = sp.GetRequiredService<ILogger<OrderService.Messaging.RabbitMqProducer>>();
    return new OrderService.Messaging.RabbitMqProducer(hostName, logger);
});

var app = builder.Build();

app.UseKargoServiceDefaults();

app.Run();
