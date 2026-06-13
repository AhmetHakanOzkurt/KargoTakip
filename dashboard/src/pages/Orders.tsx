import { useState, useEffect } from 'react';
import { getOrders, updateOrderStatus } from '../services/api';
import { Order } from '../types';

export default function Orders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [newStatus, setNewStatus] = useState('');
  const [updating, setUpdating] = useState(false);

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    try {
      const res = await getOrders();
      setOrders(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async () => {
    if (!selectedOrder || !newStatus) return;
    setUpdating(true);
    try {
      await updateOrderStatus(selectedOrder.id, {
        newStatus,
        changedByUserId: user.userId || 1,
        serviceSource: 'Dashboard'
      });
      await fetchOrders();
      setSelectedOrder(null);
      setNewStatus('');
    } catch (err) {
      console.error(err);
    } finally {
      setUpdating(false);
    }
  };

  const filtered = orders.filter(o => {
    const matchSearch = search === '' ||
      o.trackingCode.toLowerCase().includes(search.toLowerCase()) ||
      o.receiverName.toLowerCase().includes(search.toLowerCase()) ||
      o.senderName.toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === '' || o.currentStatus === statusFilter;
    return matchSearch && matchStatus;
  });

  const statusColors: Record<string, string> = {
    'Hazırlanıyor': '#1890ff',
    'Yolda': '#fa8c16',
    'Dağıtımda': '#722ed1',
    'Teslim Edildi': '#52c41a',
    'İptal': '#ff4d4f'
  };

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div>
      {/* Filtreler */}
      <div style={{ display: 'flex', gap: '12px', marginBottom: '20px' }}>
        <input
          placeholder="Takip kodu, gönderici veya alıcı ara..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          style={{
            flex: 1, padding: '10px 12px', border: '1px solid #ddd',
            borderRadius: '8px', fontSize: '14px'
          }}
        />
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          style={{
            padding: '10px 12px', border: '1px solid #ddd',
            borderRadius: '8px', fontSize: '14px', background: 'white'
          }}
        >
          <option value="">Tüm Durumlar</option>
          <option value="Hazırlanıyor">Hazırlanıyor</option>
          <option value="Yolda">Yolda</option>
          <option value="Dağıtımda">Dağıtımda</option>
          <option value="Teslim Edildi">Teslim Edildi</option>
          <option value="İptal">İptal</option>
        </select>
        <div style={{
          padding: '10px 16px', background: '#f0f0f0',
          borderRadius: '8px', fontSize: '14px', color: '#666'
        }}>
          {filtered.length} kargo
        </div>
      </div>

      {/* Tablo */}
      <div style={{ background: 'white', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', overflow: 'hidden' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#fafafa', borderBottom: '2px solid #f0f0f0' }}>
              {['Takip Kodu', 'Gönderici', 'Alıcı', 'Hedef Şehir', 'Ağırlık', 'Öncelik', 'Durum', 'Araç', 'İşlem'].map(h => (
                <th key={h} style={{ padding: '12px 16px', textAlign: 'left', color: '#666', fontWeight: 600, fontSize: '13px' }}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filtered.map(order => (
              <tr key={order.id} style={{ borderBottom: '1px solid #f0f0f0' }}
                onMouseEnter={e => (e.currentTarget.style.background = '#fafafa')}
                onMouseLeave={e => (e.currentTarget.style.background = 'white')}
              >
                <td style={{ padding: '12px 16px' }}>
                  <span style={{ fontFamily: 'monospace', fontWeight: 600, color: '#1890ff' }}>
                    {order.trackingCode}
                  </span>
                </td>
                <td style={{ padding: '12px 16px', fontSize: '14px' }}>{order.senderName}</td>
                <td style={{ padding: '12px 16px', fontSize: '14px' }}>{order.receiverName}</td>
                <td style={{ padding: '12px 16px', fontSize: '14px' }}>{order.receiverCity}</td>
                <td style={{ padding: '12px 16px', fontSize: '14px' }}>{order.weight} kg</td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{
                    padding: '3px 8px', borderRadius: '4px', fontSize: '12px',
                    background: order.priority === 'Express' ? '#fff1f0' : order.priority === 'Acil' ? '#fff7e6' : '#f6ffed',
                    color: order.priority === 'Express' ? '#ff4d4f' : order.priority === 'Acil' ? '#fa8c16' : '#52c41a'
                  }}>
                    {order.priority}
                  </span>
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{
                    padding: '4px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                    background: `${statusColors[order.currentStatus]}20`,
                    color: statusColors[order.currentStatus]
                  }}>
                    {order.currentStatus}
                  </span>
                </td>
                <td style={{ padding: '12px 16px', fontSize: '13px', color: '#666' }}>
                  {order.assignedVehicle ?? '—'}
                </td>
                <td style={{ padding: '12px 16px' }}>
                  {order.currentStatus !== 'Teslim Edildi' && order.currentStatus !== 'İptal' && (
                    <button
                      onClick={() => { setSelectedOrder(order); setNewStatus(''); }}
                      style={{
                        padding: '6px 12px', background: '#1890ff', color: 'white',
                        border: 'none', borderRadius: '6px', cursor: 'pointer', fontSize: '13px'
                      }}
                    >
                      Güncelle
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {filtered.length === 0 && (
          <div style={{ padding: '40px', textAlign: 'center', color: '#999' }}>
            Kargo bulunamadı.
          </div>
        )}
      </div>

      {/* Durum güncelleme modal */}
      {selectedOrder && (
        <div style={{
          position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }}>
          <div style={{
            background: 'white', borderRadius: '12px', padding: '32px', width: '400px',
            boxShadow: '0 8px 32px rgba(0,0,0,0.2)'
          }}>
            <h3 style={{ marginBottom: '8px' }}>Durum Güncelle</h3>
            <p style={{ color: '#666', marginBottom: '20px', fontSize: '14px' }}>
              {selectedOrder.trackingCode} — {selectedOrder.currentStatus}
            </p>

            <select
              value={newStatus}
              onChange={e => setNewStatus(e.target.value)}
              style={{
                width: '100%', padding: '10px 12px', border: '1px solid #ddd',
                borderRadius: '8px', fontSize: '14px', marginBottom: '16px',
                boxSizing: 'border-box' as any
              }}
            >
              <option value="">Yeni durum seç...</option>
              <option value="Hazırlanıyor">Hazırlanıyor</option>
              <option value="Yolda">Yolda</option>
              <option value="Dağıtımda">Dağıtımda</option>
              <option value="Teslim Edildi">Teslim Edildi</option>
              <option value="İptal">İptal</option>
            </select>

            <div style={{ display: 'flex', gap: '12px' }}>
              <button
                onClick={() => setSelectedOrder(null)}
                style={{
                  flex: 1, padding: '10px', background: '#f0f0f0',
                  border: 'none', borderRadius: '8px', cursor: 'pointer'
                }}
              >
                İptal
              </button>
              <button
                onClick={handleUpdateStatus}
                disabled={!newStatus || updating}
                style={{
                  flex: 1, padding: '10px',
                  background: !newStatus || updating ? '#ccc' : '#1890ff',
                  color: 'white', border: 'none', borderRadius: '8px',
                  cursor: !newStatus || updating ? 'not-allowed' : 'pointer'
                }}
              >
                {updating ? 'Güncelleniyor...' : 'Güncelle'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}