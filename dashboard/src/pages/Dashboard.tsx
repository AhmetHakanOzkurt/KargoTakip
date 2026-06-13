import { useState, useEffect } from 'react';
import { getSummary, getDailyReport, getConsolidationSavings, getBranchSummary } from '../services/api';

export default function Dashboard() {
  const [summary, setSummary] = useState<any>(null);
  const [daily, setDaily] = useState<any>(null);
  const [savings, setSavings] = useState<any>(null);
  const [branches, setBranches] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [summaryRes, dailyRes, savingsRes, branchRes] = await Promise.all([
          getSummary(),
          getDailyReport(),
          getConsolidationSavings(),
          getBranchSummary()
        ]);
        setSummary(summaryRes.data);
        setDaily(dailyRes.data);
        setSavings(savingsRes.data);
        setBranches(branchRes.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchAll();
  }, []);

  if (loading) return <div style={{ padding: '40px' }}>Yükleniyor...</div>;

  return (
    <div>
      <h2 style={{ marginBottom: '24px', color: '#1a1a2e' }}>📊 Genel Bakış</h2>

      {/* Üst kartlar */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '32px' }}>
        <StatCard title="Toplam Kargo" value={summary?.toplamKargo ?? 0} color="#1890ff" icon="📦" />
        <StatCard title="Bugün Oluşturulan" value={daily?.bugunOlusturulan ?? 0} color="#52c41a" icon="📝" />
        <StatCard title="Bugün Teslim" value={daily?.bugunTeslimEdilen ?? 0} color="#faad14" icon="✅" />
        <StatCard title="Aktif Araç" value={daily?.aktifAracSayisi ?? 0} color="#eb2f96" icon="🚛" />
      </div>

      {/* Konsolidasyon tasarruf kartları */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '32px' }}>
        <StatCard title="Toplam Sefer" value={savings?.toplamSefer ?? 0} color="#722ed1" icon="🗺️" />
        <StatCard title="Konsolide Kargo" value={savings?.toplamKargo ?? 0} color="#13c2c2" icon="📬" />
        <StatCard
          title="Tahmini Tasarruf"
          value={`₺${(savings?.toplamTahminiTasarruf ?? 0).toLocaleString('tr-TR', { maximumFractionDigits: 0 })}`}
          color="#52c41a"
          icon="💰"
        />
        <StatCard
          title="Ort. Doluluk"
          value={`%${savings?.ortalamaDolulukOrani ?? 0}`}
          color="#fa8c16"
          icon="📈"
        />
      </div>

      {/* Durum dağılımı */}
      {summary?.durumDagilimi && (
        <div style={{ background: 'white', borderRadius: '12px', padding: '24px', marginBottom: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
          <h3 style={{ marginBottom: '16px' }}>Kargo Durum Dağılımı</h3>
          <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
            {summary.durumDagilimi.map((item: any) => (
              <div key={item.durum} style={{
                background: getStatusColor(item.durum),
                color: 'white',
                padding: '12px 20px',
                borderRadius: '8px',
                minWidth: '120px',
                textAlign: 'center'
              }}>
                <div style={{ fontSize: '24px', fontWeight: 700 }}>{item.adet}</div>
                <div style={{ fontSize: '13px', marginTop: '4px' }}>{item.durum}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Şube doluluk tablosu */}
      <div style={{ background: 'white', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
        <h3 style={{ marginBottom: '16px' }}>Şube Durumları</h3>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #f0f0f0' }}>
              {['Şube', 'Şehir', 'Aktif Kargo', 'Toplam Araç', 'Müsait', 'Doluluk'].map(h => (
                <th key={h} style={{ padding: '10px', textAlign: 'left', color: '#666', fontWeight: 600 }}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {branches.map((b: any) => (
              <tr key={b.subeId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={{ padding: '10px', fontWeight: 500 }}>{b.subeAdi}</td>
                <td style={{ padding: '10px', color: '#666' }}>{b.sehir}</td>
                <td style={{ padding: '10px' }}>{b.aktifKargo}</td>
                <td style={{ padding: '10px' }}>{b.toplamArac}</td>
                <td style={{ padding: '10px', color: b.müsaitArac > 0 ? '#52c41a' : '#ff4d4f' }}>
                  {b.müsaitArac}
                </td>
                <td style={{ padding: '10px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <div style={{ flex: 1, background: '#f0f0f0', borderRadius: '4px', height: '8px' }}>
                      <div style={{
                        width: `${b.dolulukOrani}%`,
                        background: b.dolulukOrani > 70 ? '#ff4d4f' : b.dolulukOrani > 40 ? '#faad14' : '#52c41a',
                        height: '8px',
                        borderRadius: '4px'
                      }} />
                    </div>
                    <span style={{ fontSize: '13px', color: '#666' }}>%{b.dolulukOrani}</span>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function StatCard({ title, value, color, icon }: { title: string; value: any; color: string; icon: string }) {
  return (
    <div style={{
      background: 'white',
      borderRadius: '12px',
      padding: '20px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
      borderLeft: `4px solid ${color}`
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <div style={{ color: '#666', fontSize: '13px', marginBottom: '8px' }}>{title}</div>
          <div style={{ fontSize: '28px', fontWeight: 700, color: '#1a1a2e' }}>{value}</div>
        </div>
        <div style={{ fontSize: '32px' }}>{icon}</div>
      </div>
    </div>
  );
}

function getStatusColor(status: string) {
  const colors: Record<string, string> = {
    'Hazırlanıyor': '#1890ff',
    'Yolda': '#fa8c16',
    'Dağıtımda': '#722ed1',
    'Teslim Edildi': '#52c41a',
    'İptal': '#ff4d4f'
  };
  return colors[status] ?? '#666';
}