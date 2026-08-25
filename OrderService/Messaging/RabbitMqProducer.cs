using KargoTakip.Infrastructure.Messaging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Messaging
{
    /// <summary>
    /// Onceki surum her PublishAsync cagrisinda yeni connection ve channel
    /// aciyordu; bu hem pahali hem de yogunlukta port tukenmesine yol aciyordu.
    /// Baglanti ve kanal artik tek sefer kurulup yeniden kullaniliyor.
    /// Singleton olarak kaydedilmelidir.
    /// </summary>
    public class RabbitMqProducer : IAsyncDisposable
    {
        private readonly string _hostName;
        private readonly ILogger<RabbitMqProducer> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly HashSet<string> _bildirilenKuyruklar = new();

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqProducer(string hostName, ILogger<RabbitMqProducer> logger)
        {
            _hostName = hostName;
            _logger = logger;
        }

        private async Task<IChannel> KanalAlAsync()
        {
            if (_channel is { IsOpen: true })
                return _channel;

            if (_connection is not null && !_connection.IsOpen)
            {
                await SessizceKapatAsync();
            }

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            _connection ??= await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Baglanti yenilendiyse kuyruk tanimlari tekrar yapilmali
            _bildirilenKuyruklar.Clear();

            return _channel;
        }

        public Task PublishAsync(string queueName, object message) =>
            PublishRawAsync(queueName, JsonSerializer.Serialize(message));

        /// <summary>
        /// Onceden serilestirilmis govdeyi yayinlar. Outbox kayitlari zaten
        /// JSON tuttugu icin tekrar serilestirilmelerine gerek yoktur.
        /// </summary>
        public async Task PublishRawAsync(string queueName, string json)
        {
            await _gate.WaitAsync();
            try
            {
                var channel = await KanalAlAsync();

                if (_bildirilenKuyruklar.Add(queueName))
                {
                    // Argumanlar tuketiciyle birebir ayni olmali; farkli olursa
                    // RabbitMQ PRECONDITION_FAILED ile publish'i reddeder.
                    await channel.ExchangeDeclareAsync(
                        exchange: KuyrukTopolojisi.DeadLetterExchange,
                        type: ExchangeType.Direct,
                        durable: true,
                        autoDelete: false
                    );

                    await channel.QueueDeclareAsync(
                        queue: KuyrukTopolojisi.DlqAdi(queueName),
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    await channel.QueueBindAsync(
                        queue: KuyrukTopolojisi.DlqAdi(queueName),
                        exchange: KuyrukTopolojisi.DeadLetterExchange,
                        routingKey: KuyrukTopolojisi.DlqAdi(queueName)
                    );

                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: KuyrukTopolojisi.Argumanlar(queueName)
                    );
                }

                var body = Encoding.UTF8.GetBytes(json);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = Guid.NewGuid().ToString()
                };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body
                );
            }
            catch (Exception ex)
            {
                // Kanal bozulmus olabilir; bir sonraki cagri yeniden kursun.
                _logger.LogError(ex,
                    "RabbitMQ'ya mesaj yayinlanamadi: {Queue}", queueName);
                await SessizceKapatAsync();
                throw;
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task SessizceKapatAsync()
        {
            try
            {
                if (_channel is not null) await _channel.DisposeAsync();
                if (_connection is not null) await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RabbitMQ kaynaklari kapatilirken hata yoksayildi.");
            }
            finally
            {
                _channel = null;
                _connection = null;
                _bildirilenKuyruklar.Clear();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await SessizceKapatAsync();
            _gate.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
