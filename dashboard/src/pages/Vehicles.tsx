import { useState, useEffect } from 'react';
import { getVehicles, getBranchSummary } from '../services/api';
import { Vehicle, BranchSummary } from '../types';

export default function Vehicles() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [branches, setBranches] = useState<BranchSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'vehicles' | 'branches'>('vehicles');

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [vehiclesRes, branchesRes] = await Promise.all([
          getVehicles(),
          getBranchSummary()
        ]);
        setVehicles(vehiclesRes.data);
        setBranches(branchesRes.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchAll();
  }, []);

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div>
      {/* Tab seçimi */}
      <div style={{ display: 'flex', gap: '8px', marginBottom: '24px' }}>
        {[
          { key: 'vehicles', label: '🚛 Araç Listesi' },
          { key: 'branches', label: '🏢 Şube Doluluk' }
        ].map(tab => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key as any)}
            style={{
              padding: '10px 20px',
              background: activeTab === tab.key ? '#1890ff' : 'white',
              color: activeTab === tab.key ? 'white' : '#666',
              border: '1px solid',
              borderColor: activeTab === tab.key ? '#1890ff' : '#ddd',
              borderRadius: '8px',
              cursor: 'pointer',
              fontWeight: activeTab === tab.key ? 600 : 400
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === 'vehicles' && (
        <div style={{ background: 'white', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#fafafa', borderBottom: '2px solid #f0f0f0' }}>
                {['Plaka', 'Tip', 'Şube', 'Şehir', 'Kapasite', 'Mevcut Yük', 'Doluluk', 'Durum'].map(h => (
                  <th key={h} style={{ padding: '12px 16px', textAlign: 'left', color: '#666', fontWeight: 600, fontSize: '13px' }}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {vehicles.map(v => (
                <tr key={v.id} style={{ borderBottom: '1px solid #f0f0f0' }}>
                  <td style={{ padding: '12px 16px', fontWeight: 600, fontFamily: 'monospace' }}>{v.plateNumber}</td>
                  <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.vehicleType}</td>
                  <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.branch}</td>
                  <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.city}</td>
                  <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.capacity}</td>
                  <td style={{ padding: '12px 16px', fontSize: '14px' }}>{v.currentLoad}</td>
                  <td style={{ padding: '12px 16px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                      <div style={{ flex: 1, background: '#f0f0f0', borderRadius: '4px', height: '8px', minWidth: '80px' }}>
                        <div style={{
                          width: `${v.occupancyRate}%`,
                          background: v.occupancyRate > 70 ? '#ff4d4f' : v.occupancyRate > 40 ? '#faad14' : '#52c41a',
                          height: '8px', borderRadius: '4px'
                        }} />
                      </div>
                      <span style={{ fontSize: '13px', color: '#666', whiteSpace: 'nowrap' }}>%{v.occupancyRate}</span>
                    </div>
                  </td>
                  <td style={{ padding: '12px 16px' }}>
                    <span style={{
                      padding: '4px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                      background: v.isAvailable ? '#f6ffed' : '#fff1f0',
                      color: v.isAvailable ? '#52c41a' : '#ff4d4f'
                    }}>
                      {v.isAvailable ? 'Müsait' : 'Meşgul'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {activeTab === 'branches' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '16px' }}>
          {branches.map(b => (
            <div key={b.subeId} style={{
              background: 'white', borderRadius: '12px', padding: '20px',
              boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '16px' }}>
                <div>
                  <div style={{ fontWeight: 700, fontSize: '15px' }}>{b.subeAdi}</div>
                  <div style={{ color: '#666', fontSize: '13px' }}>{b.sehir}</div>
                </div>
                <div style={{
                  width: '48px', height: '48px', borderRadius: '50%',
                  background: b.dolulukOrani > 70 ? '#fff1f0' : b.dolulukOrani > 40 ? '#fff7e6' : '#f6ffed',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  fontSize: '13px', fontWeight: 700,
                  color: b.dolulukOrani > 70 ? '#ff4d4f' : b.dolulukOrani > 40 ? '#fa8c16' : '#52c41a'
                }}>
                  %{b.dolulukOrani}
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px', marginBottom: '16px' }}>
                {[
                  { label: 'Aktif Kargo', value: b.aktifKargo, color: '#1890ff' },
                  { label: 'Toplam Araç', value: b.toplamArac, color: '#722ed1' },
                  { label: 'Müsait Araç', value: b.müsaitArac, color: '#52c41a' },
                  { label: 'Meşgul Araç', value: b.mesgulArac, color: '#ff4d4f' },
                ].map(item => (
                  <div key={item.label} style={{
                    background: '#fafafa', borderRadius: '8px', padding: '10px',
                    borderLeft: `3px solid ${item.color}`
                  }}>
                    <div style={{ fontSize: '11px', color: '#999', marginBottom: '2px' }}>{item.label}</div>
                    <div style={{ fontSize: '20px', fontWeight: 700, color: item.color }}>{item.value}</div>
                  </div>
                ))}
              </div>

              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '12px', color: '#666', marginBottom: '4px' }}>
                  <span>Doluluk Oranı</span>
                  <span>%{b.dolulukOrani}</span>
                </div>
                <div style={{ background: '#f0f0f0', borderRadius: '4px', height: '8px' }}>
                  <div style={{
                    width: `${b.dolulukOrani}%`,
                    background: b.dolulukOrani > 70 ? '#ff4d4f' : b.dolulukOrani > 40 ? '#faad14' : '#52c41a',
                    height: '8px', borderRadius: '4px', transition: 'width 0.3s'
                  }} />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}