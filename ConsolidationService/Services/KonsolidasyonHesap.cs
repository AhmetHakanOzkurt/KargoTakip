namespace ConsolidationService.Services
{
    /// <summary>
    /// Konsolidasyon icin saf hesaplamalar. Veritabanindan bagimsiz olduklari
    /// icin dogrudan test edilebilirler.
    /// </summary>
    public static class KonsolidasyonHesap
    {
        public const decimal Yuz100KmYakitLitre = 35m;
        public const decimal LitreFiyati = 35m;

        /// <summary>
        /// Tasarruf, konsolidasyon sayesinde YAPILMAYAN ayri seferlerden gelir.
        /// Her kargo tek basina gitseydi seferSayisi kadar sefer olurdu;
        /// konsolide edilince tek sefer yapilir.
        /// Onceki formul (1 - doluluk) ile carpiyordu, yani arac bosaldikca
        /// tasarruf artiyordu ki bu konsolidasyonun mantigina terstir.
        /// </summary>
        public static decimal YakitTasarrufu(decimal mesafeKm, int seferSayisi)
        {
            if (seferSayisi <= 1 || mesafeKm <= 0)
                return 0m;

            var seferBasinaMaliyet = mesafeKm / 100m * Yuz100KmYakitLitre * LitreFiyati;
            return (seferSayisi - 1) * seferBasinaMaliyet;
        }

        /// <summary>
        /// Doluluk orani. Kapasite asilamaz; aksi halde OccupancyRate
        /// precision(5,2) tasiyordu.
        /// </summary>
        public static decimal DolulukOrani(int kullanilan, int kapasite)
        {
            if (kapasite <= 0) return 0m;
            var oran = (decimal)kullanilan / kapasite * 100m;
            return oran > 100m ? 100m : oran;
        }
    }
}
