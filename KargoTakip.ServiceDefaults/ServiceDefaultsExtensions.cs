using System.Text;
using KargoTakip.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace KargoTakip.ServiceDefaults
{
    /// <summary>
    /// Serilog, CORS, Swagger, DbContext ve JWT kurulumu alti servisin
    /// Program.cs dosyasinda birebir kopyalanmisti. AuthService'te
    /// UseAuthentication() satirinin eksik kalmasi da bu kopyalamanin
    /// dogrudan sonucuydu. Ortak kurulum burada tek yerde toplanir.
    /// </summary>
    public static class ServiceDefaultsExtensions
    {
        public const string CorsPolicyName = "AllowDashboard";

        private static readonly string[] VarsayilanOrigins =
        {
            "http://localhost:3000",
            "http://localhost:5173"
        };

        /// <summary>Serilog'u kurar; builder olusturulmadan once cagrilmalidir.</summary>
        public static void KargoLoglamaKur()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static WebApplicationBuilder AddKargoServiceDefaults(
            this WebApplicationBuilder builder, string serviceTitle)
        {
            builder.Host.UseSerilog();

            builder.Services.AddKargoCors(builder.Configuration);
            builder.Services.AddHealthChecks();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddSingleton<YerelZaman>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddKargoSwagger(serviceTitle);
            builder.Services.AddKargoDbContext(builder.Configuration);
            builder.Services.AddKargoJwtAuthentication(builder.Configuration);
            builder.Services.AddAuthorization();

            return builder;
        }

        private static void AddKargoCors(
            this IServiceCollection services, IConfiguration configuration)
        {
            // Origin listesi artik yapilandirilabilir; onceden yalnizca
            // localhost adresleri sabit yaziliydi.
            var origins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            if (origins is null || origins.Length == 0)
                origins = VarsayilanOrigins;

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        private static void AddKargoDbContext(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<KargoTakipDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null)));
        }

        private static void AddKargoJwtAuthentication(
            this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
                throw new InvalidOperationException(
                    "JwtSettings:SecretKey tanimli degil veya 32 karakterden kisa. " +
                    "Deger JWT_SECRET ortam degiskeni ile verilmelidir.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey))
                    };
                });
        }

        private static void AddKargoSwagger(
            this IServiceCollection services, string serviceTitle)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = serviceTitle,
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        /// <summary>
        /// Migration, Swagger, hata yakalama, CORS ve kimlik dogrulama
        /// pipeline'ini kurar. Sira onemlidir: UseAuthentication daima
        /// UseAuthorization'dan once gelmelidir.
        /// </summary>
        public static WebApplication UseKargoServiceDefaults(this WebApplication app)
        {
            app.KargoMigrasyonlariUygula();

            // Swagger yalnizca gelistirmede; production'da API yuzeyini ifsa etmemeli.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseCors(CorsPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHealthChecks("/health");
            app.MapControllers();

            return app;
        }

        private static void KargoMigrasyonlariUygula(this WebApplication app)
        {
            // Alti servis birden Migrate() cagirdiginda eszamanli baslangicta
            // migration catismasi oluyordu. Production'da yalnizca
            // Database__RunMigrations=true olan servis (auth-service) uygular.
            var migrationCalistir = app.Configuration
                .GetValue<bool?>("Database:RunMigrations")
                ?? app.Environment.IsDevelopment();

            if (!migrationCalistir)
                return;

            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<ExceptionHandlingMiddleware>>();

            try
            {
                var db = scope.ServiceProvider.GetRequiredService<KargoTakipDbContext>();
                db.Database.Migrate();
            }
            catch (Exception ex)
            {
                // Onceden hata loglanip devam ediliyordu. Sonuc: SQL Server
                // acilista hazir degilse veya migration veri yuzunden
                // basarisiz olursa servis ESKI semayla saglikli gorunuyor,
                // sorun ancak calisma aninda "Invalid object name" olarak
                // ortaya cikiyordu. Artik hizli basarisiz olunur; konteyner
                // restart politikasi yeniden dener.
                logger.LogCritical(ex,
                    "Migration uygulanamadi, servis baslatilmiyor.");
                throw;
            }
        }
    }
}
