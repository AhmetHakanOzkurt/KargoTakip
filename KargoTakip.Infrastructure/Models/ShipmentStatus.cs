namespace KargoTakip.Infrastructure.Models
{
    /// <summary>
    /// Kargo durumlari onceden alti projeye dagilmis magic string'lerdi ve
    /// yazim farki sessizce hataya yol aciyordu. Tek kaynak burasidir.
    /// </summary>
    public static class ShipmentStatus
    {
        public const string Hazirlaniyor = "Hazırlanıyor";
        public const string Yolda = "Yolda";
        public const string Dagitimda = "Dağıtımda";
        public const string TeslimEdildi = "Teslim Edildi";
        public const string Iptal = "İptal";

        public static readonly string[] Hepsi =
        {
            Hazirlaniyor, Yolda, Dagitimda, TeslimEdildi, Iptal
        };

        /// <summary>Kargonun daha fazla islem gormeyecegi durumlar.</summary>
        public static readonly string[] Terminal = { TeslimEdildi, Iptal };

        public static bool Gecerli(string? durum) =>
            durum is not null && Hepsi.Contains(durum);

        public static bool TerminalMi(string? durum) =>
            durum is not null && Terminal.Contains(durum);

        /// <summary>
        /// Izin verilen durum gecisleri. Onceden "Teslim Edildi" -> "Hazirlaniyor"
        /// gibi geriye donusler engellenmiyordu.
        /// </summary>
        private static readonly Dictionary<string, string[]> Gecisler = new()
        {
            [Hazirlaniyor] = new[] { Yolda, Dagitimda, Iptal },
            [Yolda] = new[] { Dagitimda, TeslimEdildi, Iptal },
            [Dagitimda] = new[] { TeslimEdildi, Iptal },
            [TeslimEdildi] = Array.Empty<string>(),
            [Iptal] = Array.Empty<string>()
        };

        public static bool GecisGecerli(string? mevcut, string? yeni)
        {
            if (!Gecerli(mevcut) || !Gecerli(yeni)) return false;
            if (mevcut == yeni) return true;
            return Gecisler[mevcut!].Contains(yeni!);
        }
    }
}
