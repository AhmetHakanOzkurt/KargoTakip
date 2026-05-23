using ConsolidationService.Services;

namespace ConsolidationService.BackgroundServices
{
    public class ConsolidationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ConsolidationBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public ConsolidationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ConsolidationBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _configuration
                .GetValue<int>("Consolidation:CheckIntervalMinutes", 60);

            _logger.LogInformation(
                "Konsolidasyon background service başladı. " +
                "Kontrol aralığı: {Interval} dakika", intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var engine = scope.ServiceProvider
                        .GetRequiredService<ConsolidationEngine>();
                    await engine.RunAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Konsolidasyon motoru hatası.");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
    }
}