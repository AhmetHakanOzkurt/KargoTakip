namespace KargoTakip.Infrastructure.Models
{
    /// <summary>
    /// Plan durumlari magic string olarak iki serviste dagilmisti ve plan
    /// "Planlandi"dan dogrudan "Tamamlandi"ya geciyordu; araci yola cikaran
    /// ara adim ile ActualDepartureAt hic yazilmiyordu.
    /// </summary>
    public static class ConsolidationPlanStatus
    {
        public const string Planlandi = "Planlandı";
        public const string Yolda = "Yolda";
        public const string Tamamlandi = "Tamamlandı";
        public const string Iptal = "İptal";

        public static readonly string[] Hepsi =
        {
            Planlandi, Yolda, Tamamlandi, Iptal
        };

        /// <summary>Plan uzerinde daha fazla islem yapilmayacak durumlar.</summary>
        public static bool TerminalMi(string? durum) =>
            durum == Tamamlandi || durum == Iptal;
    }
}
