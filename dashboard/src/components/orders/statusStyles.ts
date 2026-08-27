/** Kargo durumlarinin renkleri. */
export const statusColors: Record<string, string> = {
  'Hazırlanıyor': '#1890ff',
  'Yolda': '#fa8c16',
  'Dağıtımda': '#722ed1',
  'Teslim Edildi': '#52c41a',
  'İptal': '#ff4d4f'
};

export const KARGO_DURUMLARI = [
  'Hazırlanıyor', 'Yolda', 'Dağıtımda', 'Teslim Edildi', 'İptal'
];

/** Oncelik rozeti renkleri; ic ice ternary yerine tablo. */
const oncelikRenkleri: Record<string, { bg: string; fg: string }> = {
  'Express': { bg: '#fff1f0', fg: '#ff4d4f' },
  'Acil': { bg: '#fff7e6', fg: '#fa8c16' }
};
const oncelikVarsayilan = { bg: '#f6ffed', fg: '#52c41a' };

export const oncelikRengi = (oncelik: string) =>
  oncelikRenkleri[oncelik] ?? oncelikVarsayilan;

/** Transfer durum rozeti; ayni ic ice ternary iki modalda tekrarliyordu. */
const transferRenkleri: Record<string, { bg: string; fg: string }> = {
  'Onaylandı': { bg: '#f6ffed', fg: '#52c41a' },
  'Reddedildi': { bg: '#fff1f0', fg: '#ff4d4f' }
};
const transferVarsayilan = { bg: '#fff7e6', fg: '#fa8c16' };

export const transferDurumRengi = (durum: string) =>
  transferRenkleri[durum] ?? transferVarsayilan;

/** Bilgi/hata kutusu rengi (mesaj ✅ ile basliyorsa basarili). */
export const mesajRengi = (mesaj: string) =>
  mesaj.startsWith('✅')
    ? { bg: '#f6ffed', fg: '#52c41a' }
    : { bg: '#fff1f0', fg: '#ff4d4f' };
