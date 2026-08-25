using Microsoft.Extensions.Configuration;

namespace KargoTakip.ServiceDefaults
{
    /// <summary>
    /// Gunluk raporlar DateTime.UtcNow.Date ile hesaplaniyordu. Turkiye UTC+3
    /// oldugu icin gunun ilk 3 saatinde olusan kayitlar bir onceki gune
    /// dusuyordu. Gun sinirlari artik yerel saat diliminde hesaplanir.
    /// </summary>
    public class YerelZaman
    {
        // Turkiye 2016'dan beri yaz saati uygulamiyor, sabit UTC+3.
        // .NET 6+ IANA kimligini Windows'ta da cozer.
        private const string VarsayilanSaatDilimi = "Europe/Istanbul";

        private readonly TimeZoneInfo _saatDilimi;

        public YerelZaman(IConfiguration configuration)
        {
            var id = configuration["App:TimeZone"] ?? VarsayilanSaatDilimi;

            try
            {
                _saatDilimi = TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (Exception ex) when (
                ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                // Saat dilimi verisi eksik bir imajda servis ayaga kalkmali;
                // UTC+3 sabitine dusulur.
                _saatDilimi = TimeZoneInfo.CreateCustomTimeZone(
                    "KargoTakip-UTC+3", TimeSpan.FromHours(3), "UTC+3", "UTC+3");
            }
        }

        public string SaatDilimiAdi => _saatDilimi.Id;

        public DateTime Simdi => TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, _saatDilimi);

        /// <summary>Bugunun yerel saatle baslangici, UTC olarak dondurulur.</summary>
        public DateTime BugunBaslangicUtc()
        {
            var yerelGunBasi = Simdi.Date;
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(yerelGunBasi, DateTimeKind.Unspecified),
                _saatDilimi);
        }

        public DateTime BugunBitisUtc() => BugunBaslangicUtc().AddDays(1);

        /// <summary>Rapor basligi icin yerel tarih.</summary>
        public string BugunYerelTarih() => Simdi.ToString("yyyy-MM-dd");
    }
}
