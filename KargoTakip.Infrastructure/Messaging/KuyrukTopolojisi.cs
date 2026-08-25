namespace KargoTakip.Infrastructure.Messaging
{
    /// <summary>
    /// Kuyruk tanimlari uretici ve tuketici tarafinda ayri ayri yapiliyordu.
    /// Tuketiciye dead-letter argumanlari eklenince uretici ayni kuyrugu
    /// argumansiz tanimlamaya calisti ve RabbitMQ PRECONDITION_FAILED
    /// (inequivalent arg) ile publish'i reddetti. Topoloji artik tek
    /// kaynaktan gelir; iki taraf da ayni argumanlarla tanimlar.
    /// </summary>
    public static class KuyrukTopolojisi
    {
        public const string DeadLetterExchange = "kargo_dlx";
        public const string DlqSonEki = ".dlq";

        public const string KargoOlusturuldu = "kargo_olusturuldu";
        public const string KargoDurumuGuncellendi = "kargo_durumu_guncellendi";
        public const string TransferTalebiOlusturuldu = "transfer_talebi_olusturuldu";
        public const string TransferTalebiOnaylandi = "transfer_talebi_onaylandi";
        public const string TransferTalebiReddedildi = "transfer_talebi_reddedildi";

        /// <summary>NotificationService'in dinledigi kuyruklar.</summary>
        public static readonly string[] TuketilenKuyruklar =
        {
            KargoOlusturuldu,
            KargoDurumuGuncellendi
        };

        public static string DlqAdi(string kuyruk) => kuyruk + DlqSonEki;

        /// <summary>
        /// Kuyruk tanim argumanlari. Uretici ve tuketici birebir ayni
        /// sozlugu kullanmak zorundadir; en ufak fark publish'i reddettirir.
        /// </summary>
        public static Dictionary<string, object?> Argumanlar(string kuyruk) => new()
        {
            ["x-dead-letter-exchange"] = DeadLetterExchange,
            ["x-dead-letter-routing-key"] = DlqAdi(kuyruk)
        };
    }
}
