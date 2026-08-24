import { useState } from 'react';
import axios from 'axios';

const ORDER_URL = process.env.REACT_APP_API_BASE || '';

export default function TrackingPage() {
  const [trackingCode, setTrackingCode] = useState('');
  const [result, setResult] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleTrack = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!trackingCode.trim()) return;
    setLoading(true);
    setError('');
    setResult(null);
    try {
      const res = await axios.get(`${ORDER_URL}/api/orders/track/${trackingCode.trim()}`);
      setResult(res.data);
    } catch {
      setError('Kargo bulunamadı. Takip kodunu kontrol edin.');
    } finally {
      setLoading(false);
    }
  };

  const statusConfig: Record<string, { color: string; icon: string; bg: string }> = {
    'Hazırlanıyor': { color: '#1890ff', icon: '📦', bg: '#e6f7ff' },
    'Yolda': { color: '#fa8c16', icon: '🚛', bg: '#fff7e6' },
    'Dağıtımda': { color: '#722ed1', icon: '🏠', bg: '#f9f0ff' },
    'Teslim Edildi': { color: '#52c41a', icon: '✅', bg: '#f6ffed' },
    'İptal': { color: '#ff4d4f', icon: '❌', bg: '#fff2f0' }
  };

  const allStatuses = ['Hazırlanıyor', 'Yolda', 'Dağıtımda', 'Teslim Edildi'];
  const currentIndex = allStatuses.indexOf(result?.currentStatus);

  return (
    <div style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      {/* Header */}
      <div style={{
        background: '#1a1a2e', padding: '20px 40px',
        display: 'flex', alignItems: 'center', justifyContent: 'space-between'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <span style={{ fontSize: '28px' }}>🚚</span>
          <span style={{ color: 'white', fontWeight: 700, fontSize: '20px' }}>KargoTakip</span>
        </div>
        <a href="/" style={{ color: 'rgba(255,255,255,0.7)', fontSize: '14px', textDecoration: 'none' }}>
          Yönetim Paneli →
        </a>
      </div>

      <div style={{ maxWidth: '700px', margin: '0 auto', padding: '40px 20px' }}>
        {/* Arama kutusu */}
        <div style={{
          background: 'white', borderRadius: '16px', padding: '32px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)', marginBottom: '24px'
        }}>
          <h1 style={{ margin: '0 0 8px', fontSize: '24px', color: '#1a1a2e' }}>
            Kargo Takip
          </h1>
          <p style={{ color: '#666', margin: '0 0 24px', fontSize: '15px' }}>
            Takip kodunuzu girerek kargonuzun durumunu öğrenin.
          </p>

          <form onSubmit={handleTrack}>
            <div style={{ display: 'flex', gap: '12px' }}>
              <input
                value={trackingCode}
                onChange={e => setTrackingCode(e.target.value.toUpperCase())}
                placeholder="KRG-XXXXXXXX"
                style={{
                  flex: 1, padding: '14px 16px', border: '2px solid #ddd',
                  borderRadius: '10px', fontSize: '16px', fontFamily: 'monospace',
                  letterSpacing: '2px', outline: 'none'
                }}
                onFocus={e => e.target.style.borderColor = '#1890ff'}
                onBlur={e => e.target.style.borderColor = '#ddd'}
              />
              <button
                type="submit"
                disabled={loading || !trackingCode.trim()}
                style={{
                  padding: '14px 24px',
                  background: loading || !trackingCode.trim() ? '#ccc' : '#1890ff',
                  color: 'white', border: 'none', borderRadius: '10px',
                  fontSize: '16px', fontWeight: 600,
                  cursor: loading || !trackingCode.trim() ? 'not-allowed' : 'pointer',
                  whiteSpace: 'nowrap'
                }}
              >
                {loading ? '⏳' : '🔍 Sorgula'}
              </button>
            </div>
          </form>

          {error && (
            <div style={{
              background: '#fff2f0', border: '1px solid #ffccc7',
              borderRadius: '8px', padding: '12px 16px', marginTop: '16px',
              color: '#ff4d4f', fontSize: '14px'
            }}>
              ❌ {error}
            </div>
          )}
        </div>

        {/* Sonuç */}
        {result && (
          <div>
            {/* Durum kartı */}
            <div style={{
              background: 'white', borderRadius: '16px', padding: '24px',
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)', marginBottom: '16px'
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '24px' }}>
                <div>
                  <div style={{ fontFamily: 'monospace', color: '#1890ff', fontWeight: 700, fontSize: '18px', marginBottom: '4px' }}>
                    {result.trackingCode}
                  </div>
                  <div style={{ color: '#666', fontSize: '14px' }}>
                    {new Date(result.createdAt).toLocaleDateString('tr-TR', {
                      day: '2-digit', month: 'long', year: 'numeric'
                    })} tarihinde oluşturuldu
                  </div>
                </div>
                <div style={{
                  padding: '8px 16px', borderRadius: '20px', fontSize: '14px', fontWeight: 600,
                  background: statusConfig[result.currentStatus]?.bg ?? '#f0f0f0',
                  color: statusConfig[result.currentStatus]?.color ?? '#666'
                }}>
                  {statusConfig[result.currentStatus]?.icon} {result.currentStatus}
                </div>
              </div>

              {/* Progress bar */}
              <div style={{ marginBottom: '24px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                  {allStatuses.map((status, i) => (
                    <div key={status} style={{ textAlign: 'center', flex: 1 }}>
                      <div style={{
                        width: '32px', height: '32px', borderRadius: '50%',
                        background: i <= currentIndex ? '#1890ff' : '#f0f0f0',
                        color: i <= currentIndex ? 'white' : '#999',
                        display: 'flex', alignItems: 'center', justifyContent: 'center',
                        margin: '0 auto 6px', fontSize: '14px',
                        border: i === currentIndex ? '3px solid #1890ff' : 'none',
                        boxShadow: i === currentIndex ? '0 0 0 4px rgba(24,144,255,0.2)' : 'none'
                      }}>
                        {i <= currentIndex ? '✓' : i + 1}
                      </div>
                      <div style={{ fontSize: '11px', color: i <= currentIndex ? '#1890ff' : '#999' }}>
                        {status}
                      </div>
                    </div>
                  ))}
                </div>
                <div style={{ position: 'relative', height: '4px', background: '#f0f0f0', borderRadius: '2px', margin: '-28px 16px 0' }}>
                  <div style={{
                    position: 'absolute', left: 0, top: 0, height: '100%',
                    background: '#1890ff', borderRadius: '2px',
                    width: `${currentIndex >= 0 ? (currentIndex / (allStatuses.length - 1)) * 100 : 0}%`,
                    transition: 'width 0.5s'
                  }} />
                </div>
              </div>

              {/* Kargo bilgileri */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                {[
                  { label: '👤 Alıcı', value: result.receiverName },
                  { label: '📍 Teslimat Adresi', value: `${result.receiverAddress}, ${result.receiverCity}` },
                  { label: '⚖️ Ağırlık', value: `${result.weight} kg` },
                  { label: '🏢 Şube', value: result.branch },
                ].map(item => (
                  <div key={item.label} style={{
                    background: '#fafafa', borderRadius: '8px', padding: '12px'
                  }}>
                    <div style={{ fontSize: '12px', color: '#999', marginBottom: '4px' }}>{item.label}</div>
                    <div style={{ fontSize: '14px', fontWeight: 500, color: '#333' }}>{item.value}</div>
                  </div>
                ))}
              </div>
            </div>

            {/* Durum geçmişi */}
            <div style={{
              background: 'white', borderRadius: '16px', padding: '24px',
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)'
            }}>
              <h3 style={{ margin: '0 0 20px', fontSize: '16px' }}>📋 Kargo Geçmişi</h3>
              <div style={{ position: 'relative' }}>
                {result.statusHistory.map((h: any, i: number) => (
                  <div key={i} style={{ display: 'flex', gap: '16px', marginBottom: '16px' }}>
                    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                      <div style={{
                        width: '12px', height: '12px', borderRadius: '50%',
                        background: i === result.statusHistory.length - 1 ? '#1890ff' : '#d9d9d9',
                        flexShrink: 0, marginTop: '4px'
                      }} />
                      {i < result.statusHistory.length - 1 && (
                        <div style={{ width: '2px', flex: 1, background: '#f0f0f0', margin: '4px 0' }} />
                      )}
                    </div>
                    <div style={{ paddingBottom: i < result.statusHistory.length - 1 ? '16px' : 0 }}>
                      <div style={{ fontWeight: 600, fontSize: '14px', color: '#333', marginBottom: '2px' }}>
                        {h.status}
                      </div>
                      {h.note && (
                        <div style={{ fontSize: '13px', color: '#666', marginBottom: '2px' }}>{h.note}</div>
                      )}
                      <div style={{ fontSize: '12px', color: '#999' }}>
                        {new Date(h.changedAt).toLocaleString('tr-TR')}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}