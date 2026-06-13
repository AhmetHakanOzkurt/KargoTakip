import { useState, useEffect } from 'react';
import { getConsolidationPlans, getConsolidationSavings, runConsolidation } from '../services/api';
import { ConsolidationPlan, ConsolidationSavings } from '../types';

export default function Consolidation() {
  const [plans, setPlans] = useState<ConsolidationPlan[]>([]);
  const [savings, setSavings] = useState<ConsolidationSavings | null>(null);
  const [loading, setLoading] = useState(true);
  const [running, setRunning] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    fetchAll();
  }, []);

  const fetchAll = async () => {
    try {
      const [plansRes, savingsRes] = await Promise.all([
        getConsolidationPlans(),
        getConsolidationSavings()
      ]);
      setPlans(plansRes.data);
      setSavings(savingsRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleRun = async () => {
    setRunning(true);
    setMessage('');
    try {
      await runConsolidation();
      setMessage('✅ Motor çalıştırıldı, planlar güncelleniyor...');
      setTimeout(async () => {
        await fetchAll();
        setMessage('');
      }, 2000);
    } catch {
      setMessage('❌ Motor çalıştırılamadı.');
    } finally {
      setRunning(false);
    }
  };

  const statusColors: Record<string, string> = {
    'Planlandı': '#1890ff',
    'Yolda': '#fa8c16',
    'Tamamlandı': '#52c41a'
  };

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div>
      {/* Tasarruf kartları */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '24px' }}>
        {[
          { title: 'Toplam Sefer', value: savings?.toplamSefer ?? 0, icon: '🗺️', color: '#722ed1' },
          { title: 'Konsolide Kargo', value: savings?.toplamKargo ?? 0, icon: '📦', color: '#1890ff' },
          {
            title: 'Toplam Tasarruf',
            value: `₺${(savings?.toplamTahminiTasarruf ?? 0).toLocaleString('tr-TR', { maximumFractionDigits: 0 })}`,
            icon: '💰',
            color: '#52c41a'
          },
          {
            title: 'Ort. Doluluk',
            value: `%${savings?.ortalamaDolulukOrani ?? 0}`,
            icon: '📈',
            color: '#fa8c16'
          }
        ].map(card => (
          <div key={card.title} style={{
            background: 'white', borderRadius: '12px', padding: '20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)', borderLeft: `4px solid ${card.color}`
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ color: '#666', fontSize: '13px', marginBottom: '8px' }}>{card.title}</div>
                <div style={{ fontSize: '28px', fontWeight: 700, color: '#1a1a2e' }}>{card.value}</div>
              </div>
              <div style={{ fontSize: '32px' }}>{card.icon}</div>
            </div>
          </div>
        ))}
      </div>

      {/* Motor kontrol */}
      <div style={{
        background: 'white', borderRadius: '12px', padding: '20px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.06)', marginBottom: '24px',
        display: 'flex', alignItems: 'center', justifyContent: 'space-between'
      }}>
        <div>
          <div style={{ fontWeight: 600, fontSize: '16px', marginBottom: '4px' }}>
            🔄 Konsolidasyon Motoru
          </div>
          <div style={{ color: '#666', fontSize: '14px' }}>
            Motor otomatik olarak saatte bir çalışır. Manuel tetiklemek için butona tıklayın.
          </div>
          {message && (
            <div style={{ marginTop: '8px', fontSize: '14px', color: message.includes('✅') ? '#52c41a' : '#ff4d4f' }}>
              {message}
            </div>
          )}
        </div>
        <button
          onClick={handleRun}
          disabled={running}
          style={{
            padding: '12px 24px',
            background: running ? '#ccc' : '#1890ff',
            color: 'white', border: 'none', borderRadius: '8px',
            cursor: running ? 'not-allowed' : 'pointer',
            fontWeight: 600, fontSize: '14px', whiteSpace: 'nowrap'
          }}
        >
          {running ? '⏳ Çalışıyor...' : '▶ Motoru Çalıştır'}
        </button>
      </div>

      {/* Plan listesi */}
      <div style={{ background: 'white', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', overflow: 'hidden' }}>
        <div style={{ padding: '16px 20px', borderBottom: '1px solid #f0f0f0', fontWeight: 600 }}>
          Sefer Planları ({plans.length})
        </div>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#fafafa', borderBottom: '2px solid #f0f0f0' }}>
              {['Plan #', 'Araç', 'Çıkış Şubesi', 'Hedef Şehir', 'Kargo', 'Kapasite', 'Doluluk', 'Tasarruf', 'Durum', 'Planlanan'].map(h => (
                <th key={h} style={{ padding: '12px 16px', textAlign: 'left', color: '#666', fontWeight: 600, fontSize: '13px' }}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {plans.map(plan => (
              <tr key={plan.id} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={{ padding: '12px 16px', fontWeight: 700, color: '#1890ff' }}>#{plan.id}</td>
                <td style={{ padding: '12px 16px', fontFamily: 'monospace', fontWeight: 600 }}>{plan.arac}</td>
                <td style={{ padding: '12px 16px', fontSize: '14px' }}>{plan.cikisSube}</td>
                <td style={{ padding: '12px 16px', fontSize: '14px', fontWeight: 500 }}>{plan.hedefSehir}</td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{
                    background: '#e6f7ff', color: '#1890ff',
                    padding: '3px 8px', borderRadius: '4px', fontSize: '13px', fontWeight: 600
                  }}>
                    {plan.kargoSayisi} adet
                  </span>
                </td>
                <td style={{ padding: '12px 16px', fontSize: '14px', color: '#666' }}>
                  {plan.usedCapacity}/{plan.totalCapacity}
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <div style={{ width: '60px', background: '#f0f0f0', borderRadius: '4px', height: '8px' }}>
                      <div style={{
                        width: `${plan.dolulukOrani}%`,
                        background: plan.dolulukOrani > 70 ? '#52c41a' : plan.dolulukOrani > 40 ? '#faad14' : '#ff4d4f',
                        height: '8px', borderRadius: '4px'
                      }} />
                    </div>
                    <span style={{ fontSize: '13px' }}>%{Number(plan.dolulukOrani).toFixed(0)}</span>
                  </div>
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{ color: '#52c41a', fontWeight: 600, fontSize: '14px' }}>
                    ₺{Number(plan.tahminiYakitTasarrufu).toLocaleString('tr-TR', { maximumFractionDigits: 0 })}
                  </span>
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{
                    padding: '4px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                    background: `${statusColors[plan.status] ?? '#666'}20`,
                    color: statusColors[plan.status] ?? '#666'
                  }}>
                    {plan.status}
                  </span>
                </td>
                <td style={{ padding: '12px 16px', fontSize: '13px', color: '#666' }}>
                  {new Date(plan.plannedDepartureAt).toLocaleString('tr-TR', {
                    day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit'
                  })}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {plans.length === 0 && (
          <div style={{ padding: '40px', textAlign: 'center', color: '#999' }}>
            Henüz sefer planı yok. Motoru çalıştırın.
          </div>
        )}
      </div>
    </div>
  );
}