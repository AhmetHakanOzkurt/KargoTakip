using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        // Mail sablonlarinda http://localhost:3000 sabit yaziliydi; musteriye
        // giden linkler calismiyordu. App:PublicUrl ile yapilandirilir.
        private string TakipAdresi =>
            (_configuration["App:PublicUrl"] ?? "http://localhost:3000").TrimEnd('/');

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var fromEmail = emailSettings["FromEmail"]!;
                var fromName = emailSettings["FromName"]!;
                var smtpHost = emailSettings["SmtpHost"]!;
                var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
                var password = emailSettings["Password"]!;

                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning(
                        "EmailSettings:Password tanımlı değil, mail gönderilmedi: {Email}", toEmail);
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(fromEmail, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Mail gönderildi: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi: {Email}", toEmail);
            }
        }

        public async Task SendOrderCreatedEmailAsync(
        string toEmail, string toName, string trackingCode)
        {
            var subject = $"📦 Kargonuz Oluşturuldu — {trackingCode}";
            var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset='utf-8'></head>
            <body style='font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px;'>
              <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                <div style='background: #1a1a2e; padding: 32px; text-align: center;'>
                  <h1 style='color: white; margin: 0; font-size: 28px;'>📦 Kargonuz Oluşturuldu!</h1>
                </div>
                <div style='padding: 32px;'>
                  <p style='font-size: 16px; color: #333;'>Merhaba <strong>{toName}</strong>,</p>
                  <p style='font-size: 15px; color: #555;'>
                    Kargonuz başarıyla sisteme kaydedildi. Aşağıdaki takip kodu ile kargonuzu sorgulayabilirsiniz.
                  </p>
      
                  <div style='background: #e6f7ff; border: 2px solid #1890ff; border-radius: 12px; padding: 24px; margin: 24px 0; text-align: center;'>
                    <p style='margin: 0 0 8px; color: #666; font-size: 14px;'>TAKİP KODUNUZ</p>
                    <div style='font-size: 36px; font-weight: 900; letter-spacing: 4px; color: #1890ff;'>{trackingCode}</div>
                  </div>

                  <div style='background: #f0f7ff; border-radius: 8px; padding: 16px; margin: 16px 0;'>
                    <p style='margin: 0; color: #1890ff; font-size: 14px;'>
                      🔍 Kargonuzu takip etmek için: <strong>{TakipAdresi}/track</strong>
                    </p>
                  </div>

                  <p style='color: #333; font-size: 14px;'>İyi günler dileriz,<br><strong>KargoTakip Ekibi</strong></p>
                </div>
                <div style='background: #f5f5f5; padding: 16px; text-align: center;'>
                  <p style='margin: 0; color: #999; font-size: 12px;'>Bu mail otomatik olarak gönderilmiştir.</p>
                </div>
              </div>
            </body>
            </html>";

            await SendEmailAsync(toEmail, toName, subject, htmlBody);
        }

        public async Task SendShipmentStatusEmailAsync(
            string toEmail, string toName, string trackingCode,
            string status, string? deliveryCode = null)
        {
            var subject = status switch
            {
                "Dağıtımda" => $"🚚 Kargonuz Dağıtımda — {trackingCode}",
                "Teslim Edildi" => $"✅ Kargonuz Teslim Edildi — {trackingCode}",
                "Yolda" => $"📦 Kargonuz Yola Çıktı — {trackingCode}",
                _ => $"📦 Kargo Durumu Güncellendi — {trackingCode}"
            };

            var htmlBody = status switch
            {
                "Dağıtımda" => GetDistributionEmailTemplate(toName, trackingCode, deliveryCode!),
                "Teslim Edildi" => GetDeliveredEmailTemplate(toName, trackingCode),
                _ => GetStatusUpdateEmailTemplate(toName, trackingCode, status)
            };

            await SendEmailAsync(toEmail, toName, subject, htmlBody);
        }

        private string GetDistributionEmailTemplate(string name, string trackingCode, string deliveryCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px;'>
  <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
    <div style='background: #1890ff; padding: 32px; text-align: center;'>
      <h1 style='color: white; margin: 0; font-size: 28px;'>🚚 Kargonuz Dağıtımda!</h1>
    </div>
    <div style='padding: 32px;'>
      <p style='font-size: 16px; color: #333;'>Merhaba <strong>{name}</strong>,</p>
      <p style='font-size: 15px; color: #555;'>
        <strong>{trackingCode}</strong> takip kodlu kargonuz bugün teslim edilmek üzere dağıtıma çıkmıştır.
      </p>
      
      <div style='background: #fff7e6; border: 2px dashed #fa8c16; border-radius: 12px; padding: 24px; margin: 24px 0; text-align: center;'>
        <p style='margin: 0 0 8px; color: #666; font-size: 14px;'>TESLİMAT DOĞRULAMA KODUNUZ</p>
        <div style='font-size: 42px; font-weight: 900; letter-spacing: 8px; color: #fa8c16;'>{deliveryCode}</div>
        <p style='margin: 8px 0 0; color: #999; font-size: 12px;'>Kurye teslimatta bu kodu isteyecektir</p>
      </div>

      <div style='background: #f0f7ff; border-radius: 8px; padding: 16px; margin: 16px 0;'>
        <p style='margin: 0; color: #1890ff; font-size: 14px;'>
          ⚠️ Bu kodu kimseyle paylaşmayın. Sadece teslimatta kurye görevlisine söyleyin.
        </p>
      </div>

      <p style='color: #555; font-size: 14px;'>Sorularınız için bize ulaşabilirsiniz.</p>
      <p style='color: #333; font-size: 14px;'>İyi günler dileriz,<br><strong>KargoTakip Ekibi</strong></p>
    </div>
    <div style='background: #f5f5f5; padding: 16px; text-align: center;'>
      <p style='margin: 0; color: #999; font-size: 12px;'>Bu mail otomatik olarak gönderilmiştir.</p>
    </div>
  </div>
</body>
</html>";
        }

        private string GetDeliveredEmailTemplate(string name, string trackingCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px;'>
  <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
    <div style='background: #52c41a; padding: 32px; text-align: center;'>
      <h1 style='color: white; margin: 0; font-size: 28px;'>✅ Kargonuz Teslim Edildi!</h1>
    </div>
    <div style='padding: 32px;'>
      <p style='font-size: 16px; color: #333;'>Merhaba <strong>{name}</strong>,</p>
      <p style='font-size: 15px; color: #555;'>
        <strong>{trackingCode}</strong> takip kodlu kargonuz başarıyla teslim edilmiştir.
      </p>
      <div style='background: #f6ffed; border: 1px solid #b7eb8f; border-radius: 8px; padding: 16px; margin: 16px 0; text-align: center;'>
        <p style='margin: 0; color: #52c41a; font-size: 16px; font-weight: 600;'>
          Kargonuzu aldığınız için teşekkürler! 🎉
        </p>
      </div>
      <p style='color: #333; font-size: 14px;'>İyi günler dileriz,<br><strong>KargoTakip Ekibi</strong></p>
    </div>
  </div>
</body>
</html>";
        }

        private string GetStatusUpdateEmailTemplate(string name, string trackingCode, string status)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px;'>
  <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
    <div style='background: #1a1a2e; padding: 32px; text-align: center;'>
      <h1 style='color: white; margin: 0; font-size: 24px;'>📦 Kargo Durumu Güncellendi</h1>
    </div>
    <div style='padding: 32px;'>
      <p style='font-size: 16px; color: #333;'>Merhaba <strong>{name}</strong>,</p>
      <p style='font-size: 15px; color: #555;'>
        <strong>{trackingCode}</strong> takip kodlu kargonuzun durumu güncellendi.
      </p>
      <div style='background: #f0f0f0; border-radius: 8px; padding: 16px; margin: 16px 0; text-align: center;'>
        <p style='margin: 0; font-size: 20px; font-weight: 700; color: #1a1a2e;'>{status}</p>
      </div>
      <p style='color: #333; font-size: 14px;'>İyi günler dileriz,<br><strong>KargoTakip Ekibi</strong></p>
    </div>
  </div>
</body>
</html>";
        }
    }
}