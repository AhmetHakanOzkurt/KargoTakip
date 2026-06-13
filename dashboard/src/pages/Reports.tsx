import { useState, useEffect } from 'react';
import { getSummary, getBranchReport, getVehicleReport, getDailyReport } from '../services/api';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts';

export default function Reports() {
  const [summary, setSummary] = useState<any>(null);
  const [branchReport, setBranchReport] = useState<any[]>([]);
  const [vehicleReport, setVehicleReport] = useState<any[]>([]);
  const [daily, setDaily] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [summaryRes, branchRes, vehicleRes, dailyRes] = await Promise.all([
          getSummary(),
          getBranchReport(),
          getVehicleReport(),
          getDailyReport()
        ]);
        setSummary(summaryRes.data);
        setBranchReport(branchRes.data);
        setVehicleReport(vehicleRes.data);
        setDaily(dailyRes.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchAll();
  }, []);

  const COLORS = ['#1890ff', '#52c41a', '#fa8c16', '#722ed1', '#eb2f96', '#13c2c2'];

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div>
      {/* Tab seçimi */}
      <div style={{ display: 'flex', gap: '8px', marginBottom: '24px' }}>
        {[
          { key: 'overview', label: '📊 Genel' },
          { key: 'branches', label: '🏢 Şubeler' },
          { key: 'vehicles', label: '🚛 Araçlar' },
        ].map(tab => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            style={{
              padding: '10px 20px',
              background: activeTab === tab.key ? '#1890ff' : 'white',
              color: activeTab === tab.key ? 'white' : '#666',
              border: '1px solid',
              borderColor: activeTab === tab.key ? '#1890ff' : '#ddd',
              borderRadius: '8px', cursor: 'pointer',
              fontWeight: activeTab === tab.key ? 600 : 400
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === 'overview' && (
        <div>
          {/* Günlük özet kartları */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '24px' }}>
            {[
              { title: 'Toplam Kargo', value: summary?.toplamKargo, color: '#1890ff', icon: '📦' },
              { title: 'Bugün Oluşturulan', value: daily?.bugunOlusturulan, color: '#52c41a', icon: '📝' },
              { title: 'Bugün Teslim', value: daily?.bugunTeslimEdilen, color: '#fa8c16', icon: '✅' },
              { title: 'Toplam Ağırlık (kg)', value: summary?.toplamAgirlik, color: '#722ed1', icon: '⚖️' },
            ].map(card => (
              <div key={card.title} style={{
                background: 'white', borderRadius: '12px', padding: '20px',
                boxShadow: '0 2px 8px rgba(0,0,0,0.06)', borderLeft: `4px solid ${card.color}`
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <div style={{ color: '#666', fontSize: '13px', marginBottom: '8px' }}>{card.title}</div>
                    <div style={{ fontSize: '28px', fontWeight: 700 }}>{card.value ?? 0}</div>
                  </div>
                  <div style={{ fontSize: '32px' }}>{card.icon}</div>
                </div>
              </div>
            ))}
          </div>

          {/* Durum dağılımı pasta grafiği */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <div style={{ background: 'white', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
              <h3 style={{ marginBottom: '16px' }}>Kargo Durum Dağılımı</h3>
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={summary?.durumDagilimi?.map((d: any) => ({ name: d.durum, value: d.adet }))}
                    cx="50%" cy="50%" outerRadius={80}
                    dataKey="value" label={({ name, value }) => `${name}: ${value}`}
                  >
                    {summary?.durumDagilimi?.map((_: any, i: number) => (
                      <Cell key={i} fill={COLORS[i % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>

            <div style={{ background: 'white', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
              <h3 style={{ marginBottom: '16px' }}>Öncelik Dağılımı</h3>
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={summary?.oncelikDagilimi?.map((d: any) => ({ name: d.oncelik, value: d.adet }))}
                    cx="50%" cy="50%" outerRadius={80}
                    dataKey="value" label={({ name, value }) => `${name}: ${value}`}
                  >
                    {summary?.oncelikDagilimi?.map((_: any, i: number) => (
                      <Cell key={i} fill={COLORS[i % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'branches' && (
        <div>
          <div style={{ background: 'white', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', marginBottom: '16px' }}>
            <h3 style={{ marginBottom: '16px' }}>Şube Bazlı Kargo Dağılımı</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={branchReport.map(b => ({ name: b.subeAdi.replace(' Şubesi', ''), toplam: b.toplamKargo, teslim: b.teslimEdilen, devam: b.devamEden }))}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" fontSize={12} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="toplam" name="Toplam" fill="#1890ff" />
                <Bar dataKey="teslim" name="Teslim" fill="#52c41a" />
                <Bar dataKey="devam" name="Devam Eden" fill="#fa8c16" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div style={{ background: 'white', borderRadius: '12px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#fafafa', borderBottom: '2px solid #f0f0f0' }}>
                  {['Şube', 'Toplam Kargo', 'Teslim Edilen', 'Devam Eden', 'Toplam Ağırlık', 'Araç'].map(h => (
                    <th key={h} style={{ padding: '12px 16px', textAlign: 'left', color: '#666', fontWeight: 600, fontSize: '13px' }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {branchReport.map((b: any) => (
                  <tr key={b.subeId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                    <td style={{ padding: '12px 16px', fontWeight: 500 }}>{b.subeAdi}</td>
                    <td style={{ padding: '12px 16px' }}>{b.toplamKargo}</td>
                    <td style={{ padding: '12px 16px', color: '#52c41a', fontWeight: 500 }}>{b.teslimEdilen}</td>
                    <td style={{ padding: '12px 16px', color: '#fa8c16', fontWeight: 500 }}>{b.devamEden}</td>
                    <td style={{ padding: '12px 16px' }}>{b.toplamAgirlik} kg</td>
                    <td style={{ padding: '12px 16px' }}>{b.toplamArac}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'vehicles' && (
        <div>
          <div style={{ background: 'white', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', marginBottom: '16px' }}>
            <h3 style={{ marginBottom: '16px' }}>Araç Doluluk Oranları</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={vehicleReport.map(v => ({ name: v.plaka, doluluk: v.dolulukOrani, kargo: v.tasinanKargo }))}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" fontSize={12} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="doluluk" name="Doluluk %" fill="#1890ff" />
                <Bar dataKey="kargo" name="Taşınan Kargo" fill="#52c41a" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div style={{ background: 'white', borderRadius: '12px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#fafafa', borderBottom: '2px solid #f0f0f0' }}>
                  {['Plaka', 'Tip', 'Şube', 'Kapasite', 'Yük', 'Doluluk %', 'Taşınan', 'Durum'].map(h => (
                    <th key={h} style={{ padding: '12px 16px', textAlign: 'left', color: '#666', fontWeight: 600, fontSize: '13px' }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {vehicleReport.map((v: any) => (
                  <tr key={v.aracId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                    <td style={{ padding: '12px 16px', fontFamily: 'monospace', fontWeight: 600 }}>{v.plaka}</td>
                    <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.tip}</td>
                    <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.sube}</td>
                    <td style={{ padding: '12px 16px' }}>{v.kapasite}</td>
                    <td style={{ padding: '12px 16px' }}>{v.mevcutYuk}</td>
                    <td style={{ padding: '12px 16px' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <div style={{ width: '60px', background: '#f0f0f0', borderRadius: '4px', height: '8px' }}>
                          <div style={{
                            width: `${v.dolulukOrani}%`,
                            background: v.dolulukOrani > 70 ? '#ff4d4f' : v.dolulukOrani > 40 ? '#faad14' : '#52c41a',
                            height: '8px', borderRadius: '4px'
                          }} />
                        </div>
                        <span style={{ fontSize: '13px' }}>%{v.dolulukOrani}</span>
                      </div>
                    </td>
                    <td style={{ padding: '12px 16px' }}>{v.tasinanKargo}</td>
                    <td style={{ padding: '12px 16px' }}>
                      <span style={{
                        padding: '4px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                        background: v.müsaitMi ? '#f6ffed' : '#fff1f0',
                        color: v.müsaitMi ? '#52c41a' : '#ff4d4f'
                      }}>
                        {v.müsaitMi ? 'Müsait' : 'Meşgul'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}