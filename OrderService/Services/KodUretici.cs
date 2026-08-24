using System.Security.Cryptography;

namespace OrderService.Services
{
    /// <summary>
    /// Takip ve teslimat kodu uretimi. Controller'dan ayri tutulur ki
    /// veritabani olmadan test edilebilsin.
    /// </summary>
    public static class KodUretici
    {
        // Okuma hatasi yaratan karakterler (I, O, 0, 1) alfabeye dahil degil.
        internal const string Alfabe = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        internal const string TakipKoduOneki = "KRG-";
        internal const int TakipKoduUzunlugu = 10;

        public static string TakipKodu()
        {
            var karakterler = new char[TakipKoduUzunlugu];
            for (int i = 0; i < TakipKoduUzunlugu; i++)
                karakterler[i] = Alfabe[RandomNumberGenerator.GetInt32(Alfabe.Length)];

            return TakipKoduOneki + new string(karakterler);
        }

        // Random tahmin edilebilir oldugu icin kriptografik uretec kullanilir.
        public static string TeslimatKodu() =>
            RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}
