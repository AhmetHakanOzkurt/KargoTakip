import { useState, useEffect } from 'react';
import { getBranchSummary } from '../services/api';
import { BranchSummary } from '../types';
import { getOrders } from '../services/api';

export default function Branches() {
  const [branches, setBranches] = useState<BranchSummary[]>([]);
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedBranch, setSelectedBranch] = useState<BranchSummary | null>(null);

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [branchRes, orderRes] = await Promise.all([
          getBranchSummary(),
          getOrders()
        ]);
        setBranches(branchRes.data);
        setOrders(orderRes.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchAll();
  }, []);

  if (loading) return <div>Yükleniyor...</div>;

  const branchOrders = selectedBranch
    ? orders.filter((o: any) => o.branch === selectedBranch.subeAdi)
    : [];

  return (
    <div style={{ display: 'grid', gridTemplateColumns: selectedBranch ? '1fr 1fr' : '1fr', gap: '20px' }}>
      {/* Şube kartları */}
      <div>
        <div style={{ marginBottom: '16px', color: '#666', fontSize: '14px' }}>
          Bir şubeye tıklayarak detaylarını görebilirsiniz.
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
          {branches.map(b => (
            <div
              key={b.subeId}
              onClick={() => setSelectedBranch(selectedBranch?.subeId === b.subeId ? null : b)}
              style={{
                background: 'white', borderRadius: '12px', padding: '20px',
                boxShadow: '0 2px 8px rgba(0,0,0,0.06)', cursor: 'pointer',
                borderLeft: `4px solid ${selectedBranch?.subeId === b.subeId ? '#1890ff' : '#f0f0f0'}`,
                transition: 'all 0.2s'
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <div style={{ fontWeight: 700, fontSize: '15px', marginBottom: '4px' }}>{b.subeAdi}</div>
                  <div style={{ color: '#666', fontSize: '13px' }}>📍 {b.sehir}</div>
                </div>
                <div style={{
                  width: '52px', height: '52px', borderRadius: '50%',
                  background: b.dolulukOrani > 70 ? '#fff1f0' : b.dolulukOrani > 40 ? '#fff7e6' : '#f6ffed',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  flexDirection: 'column'
                }}>
                  <div style={{
                    fontSize: '14px', fontWeight: 700,
                    color: b.dolulukOrani > 70 ? '#ff4d4f' : b.dolulukOrani > 40 ? '#fa8c16' : '#52c41a'
                  }}>
                    %{b.dolulukOrani}
                  </div>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '8px', marginTop: '16px' }}>
                {[
                  { label: 'Aktif Kargo', value: b.aktifKargo, color: '#1890ff' },
                  { label: 'Toplam Araç', value: b.toplamArac, color: '#722ed1' },
                  { label: 'Müsait', value: b.müsaitArac, color: '#52c41a' },
                  { label: 'Meşgul', value: b.mesgulArac, color: '#ff4d4f' },
                ].map(item => (
                  <div key={item.label} style={{
                    background: '#fafafa', borderRadius: '8px', padding: '8px',
                    textAlign: 'center', borderTop: `3px solid ${item.color}`
                  }}>
                    <div style={{ fontSize: '18px', fontWeight: 700, color: item.color }}>{item.value}</div>
                    <div style={{ fontSize: '11px', color: '#999', marginTop: '2px' }}>{item.label}</div>
                  </div>
                ))}
              </div>

              <div style={{ marginTop: '12px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '12px', color: '#666', marginBottom: '4px' }}>
                  <span>Araç doluluk oranı</span>
                  <span>{b.toplamYuk}/{b.toplamKapasite}</span>
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
      </div>

      {/* Seçili şube detayı */}
      {selectedBranch && (
        <div>
          <div style={{
            background: 'white', borderRadius: '12px', padding: '20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)', marginBottom: '16px'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
              <h3 style={{ margin: 0 }}>{selectedBranch.subeAdi}</h3>
              <button
                onClick={() => setSelectedBranch(null)}
                style={{
                  background: 'none', border: 'none', fontSize: '18px',
                  cursor: 'pointer', color: '#999'
                }}
              >
                ✕
              </button>
            </div>
            <div style={{ color: '#666', fontSize: '14px', marginBottom: '16px' }}>
              📍 {selectedBranch.sehir}
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
              {[
                { label: 'Aktif Kargo', value: selectedBranch.aktifKargo, icon: '📦', color: '#1890ff' },
                { label: 'Toplam Araç', value: selectedBranch.toplamArac, icon: '🚛', color: '#722ed1' },
                { label: 'Müsait Araç', value: selectedBranch.müsaitArac, icon: '✅', color: '#52c41a' },
                { label: 'Doluluk Oranı', value: `%${selectedBranch.dolulukOrani}`, icon: '📊', color: '#fa8c16' },
              ].map(item => (
                <div key={item.label} style={{
                  background: '#fafafa', borderRadius: '8px', padding: '12px',
                  borderLeft: `3px solid ${item.color}`
                }}>
                  <div style={{ fontSize: '11px', color: '#999', marginBottom: '4px' }}>{item.label}</div>
                  <div style={{ fontSize: '22px', fontWeight: 700, color: item.color }}>
                    {item.icon} {item.value}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Şubenin kargoları */}
          <div style={{
            background: 'white', borderRadius: '12px', padding: '20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
          }}>
            <h4 style={{ marginBottom: '16px' }}>
              Aktif Kargolar ({branchOrders.length})
            </h4>
            {branchOrders.length === 0 ? (
              <div style={{ color: '#999', textAlign: 'center', padding: '20px' }}>
                Bu şubede aktif kargo yok.
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                {branchOrders.slice(0, 10).map((o: any) => (
                  <div key={o.id} style={{
                    padding: '10px 12px', background: '#fafafa',
                    borderRadius: '8px', fontSize: '13px'
                  }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span style={{ fontFamily: 'monospace', color: '#1890ff', fontWeight: 600 }}>
                        {o.trackingCode}
                      </span>
                      <span style={{
                        padding: '2px 8px', borderRadius: '4px', fontSize: '11px',
                        background: o.currentStatus === 'Teslim Edildi' ? '#f6ffed' : '#e6f7ff',
                        color: o.currentStatus === 'Teslim Edildi' ? '#52c41a' : '#1890ff'
                      }}>
                        {o.currentStatus}
                      </span>
                    </div>
                    <div style={{ color: '#666', marginTop: '4px' }}>
                      {o.receiverName} → {o.receiverCity}
                    </div>
                  </div>
                ))}
                {branchOrders.length > 10 && (
                  <div style={{ textAlign: 'center', color: '#999', fontSize: '13px', padding: '8px' }}>
                    +{branchOrders.length - 10} kargo daha...
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}