using AspNetCoreRateLimit;
using FluentValidation;
using KargoTakip.ServiceDefaults;

ServiceDefaultsExtensions.KargoLoglamaKur();

var builder = WebApplication.CreateBuilder(args);

// Serilog, CORS, Swagger, DbContext, JWT ve authorization kurulumu ortak.
builder.AddKargoServiceDefaults("AuthService");

// AuthService'e ozgu: login denemelerini sinirla
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(
    builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddValidatorsFromAssemblyContaining<AuthService.Validators.LoginRequestValidator>();

var app = builder.Build();

// Rate limiting ortak pipeline'dan once devreye girmelidir.
app.UseIpRateLimiting();

app.UseKargoServiceDefaults();

app.Run();
