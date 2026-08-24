import { useState, useEffect } from 'react';
import { getOrders, updateOrderStatus, createTransfer, getOutgoingTransfers, getIncomingTransfers, approveTransfer, rejectTransfer, getBranches } from '../services/api';
import { Order } from '../types';

export default function Orders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [newStatus, setNewStatus] = useState('');
  const [updating, setUpdating] = useState(false);

  // Transfer talebi state
  const [showTransferModal, setShowTransferModal] = useState(false);
  const [transferOrders, setTransferOrders] = useState<number[]>([]);
  const [targetBranchId, setTargetBranchId] = useState('');
  const [transferNote, setTransferNote] = useState('');
  const [branches, setBranches] = useState<any[]>([]);
  const [transferring, setTransferring] = useState(false);
  const [transferMsg, setTransferMsg] = useState('');

  // Giden transfer talepleri
  const [showOutgoing, setShowOutgoing] = useState(false);
  const [outgoing, setOutgoing] = useState<any[]>([]);

  // Gelen transfer talepleri
  const [showIncoming, setShowIncoming] = useState(false);
  const [incoming, setIncoming] = useState<any[]>([]);
  const [selectedTransfer, setSelectedTransfer] = useState<any>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [scheduleDate, setScheduleDate] = useState('');
  const [transferActionMsg, setTransferActionMsg] = useState('');

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  useEffect(() => {
    fetchOrders();
    getBranches().then(r => setBranches(r.data)).catch(() => {});
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

  const handleTransfer = async () => {
    if (!targetBranchId || transferOrders.length === 0) return;
    setTransferring(true);
    setTransferMsg('');
    try {
      await createTransfer({
        requesterBranchId: user.branchId,
        targetBranchId: parseInt(targetBranchId),
        shipmentIds: transferOrders,
        note: transferNote
      });
      setTransferMsg('✅ Transfer talebi başarıyla oluşturuldu.');
      setTransferOrders([]);
      setTargetBranchId('');
      setTransferNote('');
      await fetchOrders();
    } catch (err: any) {
      setTransferMsg('❌ ' + (err.response?.data?.message || 'Transfer talebi oluşturulamadı.'));
    } finally {
      setTransferring(false);
    }
  };

  const fetchOutgoing = async () => {
    try {
      const res = await getOutgoingTransfers();
      setOutgoing(res.data);
      setShowOutgoing(true);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchIncoming = async () => {
    try {
      const res = await getIncomingTransfers();
      setIncoming(res.data);
      setShowIncoming(true);
      setTransferActionMsg('');
    } catch (err) {
      console.error(err);
    }
  };

  const handleApprove = async () => {
    if (!selectedTransfer || !scheduleDate) return;
    try {
      await approveTransfer(selectedTransfer.id, { scheduledAt: new Date(scheduleDate).toISOString() });
      setTransferActionMsg('✅ Transfer talebi onaylandı.');
      const res = await getIncomingTransfers();
      setIncoming(res.data);
      setSelectedTransfer(null);
    } catch (err: any) {
      setTransferActionMsg('❌ ' + (err.response?.data?.message || 'Onaylanamadı.'));
    }
  };

  const handleReject = async () => {
    if (!selectedTransfer || !rejectReason) return;
    try {
      await rejectTransfer(selectedTransfer.id, { reason: rejectReason });
      setTransferActionMsg('✅ Transfer talebi reddedildi.');
      const res = await getIncomingTransfers();
      setIncoming(res.data);
      setSelectedTransfer(null);
    } catch (err: any) {
      setTransferActionMsg('❌ ' + (err.response?.data?.message || 'Reddedilemedi.'));
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

  const transferableOrders = filtered.filter(o => o.currentStatus === 'Hazırlanıyor');

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
      {/* Üst butonlar */}
      <div style={{ display: 'flex', gap: '12px', marginBottom: '20px', flexWrap: 'wrap' }}>
        <input
          placeholder="Takip kodu, gönderici veya alıcı ara..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          style={{ flex: 1, minWidth: '200px', padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px' }}
        />
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', background: 'white' }}
        >
          <option value="">Tüm Durumlar</option>
          <option value="Hazırlanıyor">Hazırlanıyor</option>
          <option value="Yolda">Yolda</option>
          <option value="Dağıtımda">Dağıtımda</option>
          <option value="Teslim Edildi">Teslim Edildi</option>
          <option value="İptal">İptal</option>
        </select>
        <button
          onClick={() => { setShowTransferModal(true); setTransferMsg(''); }}
          style={{ padding: '10px 16px', background: '#722ed1', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '14px' }}
        >
          🔄 Transfer Talebi
        </button>
        <button
          onClick={fetchOutgoing}
          style={{ padding: '10px 16px', background: '#fa8c16', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '14px' }}
        >
          📤 Giden Talepler
        </button>
        {(user.role === 'BranchManager' || user.role === 'Admin') && (
          <button
            onClick={fetchIncoming}
            style={{ padding: '10px 16px', background: '#52c41a', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '14px' }}
          >
            📥 Gelen Talepler
          </button>
        )}
        <div style={{ padding: '10px 16px', background: '#f0f0f0', borderRadius: '8px', fontSize: '14px', color: '#666' }}>
          {filtered.length} kargo
        </div>
      </div>

      {/* Kargo tablosu */}
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
                  <span style={{ fontFamily: 'monospace', fontWeight: 600, color: '#1890ff' }}>{order.trackingCode}</span>
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
                  }}>{order.priority}</span>
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{
                    padding: '4px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                    background: `${statusColors[order.currentStatus]}20`,
                    color: statusColors[order.currentStatus]
                  }}>{order.currentStatus}</span>
                </td>
                <td style={{ padding: '12px 16px', fontSize: '13px', color: '#666' }}>{order.assignedVehicle ?? '—'}</td>
                <td style={{ padding: '12px 16px' }}>
                  {order.currentStatus !== 'Teslim Edildi' && order.currentStatus !== 'İptal' && (
                    <button
                      onClick={() => { setSelectedOrder(order); setNewStatus(''); }}
                      style={{ padding: '6px 12px', background: '#1890ff', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontSize: '13px' }}
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
          <div style={{ padding: '40px', textAlign: 'center', color: '#999' }}>Kargo bulunamadı.</div>
        )}
      </div>

      {/* Durum güncelleme modal */}
      {selectedOrder && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: 'white', borderRadius: '12px', padding: '32px', width: '400px', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' }}>
            <h3 style={{ marginBottom: '8px' }}>Durum Güncelle</h3>
            <p style={{ color: '#666', marginBottom: '20px', fontSize: '14px' }}>{selectedOrder.trackingCode} — {selectedOrder.currentStatus}</p>
            <select
              value={newStatus}
              onChange={e => setNewStatus(e.target.value)}
              style={{ width: '100%', padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', marginBottom: '16px', boxSizing: 'border-box' as any }}
            >
              <option value="">Yeni durum seç...</option>
              <option value="Hazırlanıyor">Hazırlanıyor</option>
              <option value="Yolda">Yolda</option>
              <option value="Dağıtımda">Dağıtımda</option>
              <option value="Teslim Edildi">Teslim Edildi</option>
              <option value="İptal">İptal</option>
            </select>
            <div style={{ display: 'flex', gap: '12px' }}>
              <button onClick={() => setSelectedOrder(null)} style={{ flex: 1, padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>İptal</button>
              <button onClick={handleUpdateStatus} disabled={!newStatus || updating}
                style={{ flex: 1, padding: '10px', background: !newStatus || updating ? '#ccc' : '#1890ff', color: 'white', border: 'none', borderRadius: '8px', cursor: !newStatus || updating ? 'not-allowed' : 'pointer' }}>
                {updating ? 'Güncelleniyor...' : 'Güncelle'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Transfer talebi modal */}
      {showTransferModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: 'white', borderRadius: '12px', padding: '32px', width: '520px', maxHeight: '80vh', overflowY: 'auto', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' }}>
            <h3 style={{ marginBottom: '4px' }}>🔄 Transfer Talebi Oluştur</h3>
            <p style={{ color: '#666', fontSize: '13px', marginBottom: '20px' }}>Sadece "Hazırlanıyor" durumundaki kargolar transfer edilebilir.</p>

            <div style={{ marginBottom: '16px' }}>
              <label style={{ fontSize: '13px', color: '#555', fontWeight: 600 }}>Hedef Şube</label>
              <select
                value={targetBranchId}
                onChange={e => setTargetBranchId(e.target.value)}
                style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', marginTop: '6px', boxSizing: 'border-box' as any }}
              >
                <option value="">Şube seç...</option>
                {branches.filter((b: any) => b.id !== user.branchId).map((b: any) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </div>

            <div style={{ marginBottom: '16px' }}>
              <label style={{ fontSize: '13px', color: '#555', fontWeight: 600 }}>Transfer Edilecek Kargolar</label>
              <div style={{ marginTop: '8px', maxHeight: '200px', overflowY: 'auto', border: '1px solid #eee', borderRadius: '8px' }}>
                {transferableOrders.length === 0 ? (
                  <div style={{ padding: '16px', color: '#999', fontSize: '13px', textAlign: 'center' }}>Transfer edilebilir kargo yok.</div>
                ) : (
                  transferableOrders.map(o => (
                    <label key={o.id} style={{ display: 'flex', alignItems: 'center', gap: '10px', padding: '10px 14px', borderBottom: '1px solid #f0f0f0', cursor: 'pointer' }}>
                      <input
                        type="checkbox"
                        checked={transferOrders.includes(o.id)}
                        onChange={e => {
                          if (e.target.checked) setTransferOrders(prev => [...prev, o.id]);
                          else setTransferOrders(prev => prev.filter(id => id !== o.id));
                        }}
                      />
                      <span style={{ fontFamily: 'monospace', fontSize: '13px', color: '#1890ff' }}>{o.trackingCode}</span>
                      <span style={{ fontSize: '13px', color: '#666' }}>{o.receiverName} — {o.receiverCity}</span>
                    </label>
                  ))
                )}
              </div>
              {transferOrders.length > 0 && (
                <div style={{ fontSize: '12px', color: '#722ed1', marginTop: '6px' }}>{transferOrders.length} kargo seçildi</div>
              )}
            </div>

            <div style={{ marginBottom: '20px' }}>
              <label style={{ fontSize: '13px', color: '#555', fontWeight: 600 }}>Not (isteğe bağlı)</label>
              <textarea
                value={transferNote}
                onChange={e => setTransferNote(e.target.value)}
                placeholder="Transfer sebebi veya notunuz..."
                rows={3}
                style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', marginTop: '6px', resize: 'none', boxSizing: 'border-box' as any }}
              />
            </div>

            {transferMsg && (
              <div style={{ padding: '10px 14px', borderRadius: '8px', marginBottom: '16px', background: transferMsg.startsWith('✅') ? '#f6ffed' : '#fff1f0', color: transferMsg.startsWith('✅') ? '#52c41a' : '#ff4d4f', fontSize: '13px' }}>
                {transferMsg}
              </div>
            )}

            <div style={{ display: 'flex', gap: '12px' }}>
              <button onClick={() => { setShowTransferModal(false); setTransferMsg(''); setTransferOrders([]); }}
                style={{ flex: 1, padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>
                Kapat
              </button>
              <button onClick={handleTransfer} disabled={!targetBranchId || transferOrders.length === 0 || transferring}
                style={{ flex: 1, padding: '10px', background: (!targetBranchId || transferOrders.length === 0 || transferring) ? '#ccc' : '#722ed1', color: 'white', border: 'none', borderRadius: '8px', cursor: (!targetBranchId || transferOrders.length === 0 || transferring) ? 'not-allowed' : 'pointer' }}>
                {transferring ? 'Gönderiliyor...' : 'Transfer Talebi Gönder'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Giden transfer talepleri modal */}
      {showOutgoing && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: 'white', borderRadius: '12px', padding: '32px', width: '560px', maxHeight: '80vh', overflowY: 'auto', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' }}>
            <h3 style={{ marginBottom: '20px' }}>📤 Giden Transfer Talepleri</h3>
            {outgoing.length === 0 ? (
              <div style={{ color: '#999', textAlign: 'center', padding: '20px' }}>Giden transfer talebi yok.</div>
            ) : (
              outgoing.map((t: any) => (
                <div key={t.id} style={{ border: '1px solid #eee', borderRadius: '8px', padding: '16px', marginBottom: '12px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                    <span style={{ fontWeight: 600 }}>#{t.id} — {t.hedefSube}</span>
                    <span style={{
                      padding: '3px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                      background: t.status === 'Onaylandı' ? '#f6ffed' : t.status === 'Reddedildi' ? '#fff1f0' : '#fff7e6',
                      color: t.status === 'Onaylandı' ? '#52c41a' : t.status === 'Reddedildi' ? '#ff4d4f' : '#fa8c16'
                    }}>{t.status}</span>
                  </div>
                  <div style={{ fontSize: '13px', color: '#666' }}>
                    {t.kargoSayisi} kargo • {new Date(t.requestedAt).toLocaleString('tr-TR')}
                  </div>
                  {t.scheduledAt && (
                    <div style={{ fontSize: '13px', color: '#1890ff', marginTop: '4px' }}>
                      Planlanan: {new Date(t.scheduledAt).toLocaleString('tr-TR')}
                    </div>
                  )}
                </div>
              ))
            )}
            <button onClick={() => setShowOutgoing(false)}
              style={{ width: '100%', padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer', marginTop: '8px' }}>
              Kapat
            </button>
          </div>
        </div>
      )}

      {/* Gelen transfer talepleri modal */}
      {showIncoming && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: 'white', borderRadius: '12px', padding: '32px', width: '560px', maxHeight: '80vh', overflowY: 'auto', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' }}>
            <h3 style={{ marginBottom: '20px' }}>📥 Gelen Transfer Talepleri</h3>
            {transferActionMsg && (
              <div style={{ padding: '10px 14px', borderRadius: '8px', marginBottom: '16px', fontSize: '13px',
                background: transferActionMsg.startsWith('✅') ? '#f6ffed' : '#fff1f0',
                color: transferActionMsg.startsWith('✅') ? '#52c41a' : '#ff4d4f' }}>
                {transferActionMsg}
              </div>
            )}
            {incoming.length === 0 ? (
              <div style={{ color: '#999', textAlign: 'center', padding: '20px' }}>Gelen transfer talebi yok.</div>
            ) : (
              incoming.map((t: any) => (
                <div key={t.id} style={{ border: '1px solid #eee', borderRadius: '8px', padding: '16px', marginBottom: '12px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                    <span style={{ fontWeight: 600 }}>#{t.id} — {t.talepEdenSube}</span>
                    <span style={{
                      padding: '3px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
                      background: t.status === 'Onaylandı' ? '#f6ffed' : t.status === 'Reddedildi' ? '#fff1f0' : '#fff7e6',
                      color: t.status === 'Onaylandı' ? '#52c41a' : t.status === 'Reddedildi' ? '#ff4d4f' : '#fa8c16'
                    }}>{t.status}</span>
                  </div>
                  <div style={{ fontSize: '13px', color: '#666', marginBottom: '10px' }}>
                    {t.kargoSayisi} kargo • {new Date(t.requestedAt).toLocaleString('tr-TR')}
                  </div>
                  {t.status === 'Bekliyor' && (
                    <button
                      onClick={() => { setSelectedTransfer(t); setRejectReason(''); setScheduleDate(''); setTransferActionMsg(''); }}
                      style={{ padding: '6px 14px', background: '#1890ff', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontSize: '13px' }}
                    >
                      İncele / Yanıtla
                    </button>
                  )}
                  {t.scheduledAt && (
                    <div style={{ fontSize: '13px', color: '#52c41a', marginTop: '6px' }}>
                      Planlanan: {new Date(t.scheduledAt).toLocaleString('tr-TR')}
                    </div>
                  )}
                </div>
              ))
            )}
            <button onClick={() => { setShowIncoming(false); setTransferActionMsg(''); }}
              style={{ width: '100%', padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer', marginTop: '8px' }}>
              Kapat
            </button>
          </div>
        </div>
      )}

      {/* Onayla / Reddet modal */}
      {selectedTransfer && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1100 }}>
          <div style={{ background: 'white', borderRadius: '12px', padding: '32px', width: '440px', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' }}>
            <h3 style={{ marginBottom: '4px' }}>Transfer Talebi #{selectedTransfer.id}</h3>
            <p style={{ color: '#666', fontSize: '13px', marginBottom: '20px' }}>
              {selectedTransfer.talepEdenSube} şubesinden {selectedTransfer.kargoSayisi} kargo
            </p>

            <div style={{ marginBottom: '16px' }}>
              <label style={{ fontSize: '13px', color: '#555', fontWeight: 600, display: 'block', marginBottom: '6px' }}>
                Planlanan Transfer Tarihi (Onayla için)
              </label>
              <input
                type="datetime-local"
                value={scheduleDate}
                onChange={e => setScheduleDate(e.target.value)}
                style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', boxSizing: 'border-box' as any }}
              />
            </div>

            <div style={{ marginBottom: '20px' }}>
              <label style={{ fontSize: '13px', color: '#555', fontWeight: 600, display: 'block', marginBottom: '6px' }}>
                Red Sebebi (Reddet için)
              </label>
              <input
                value={rejectReason}
                onChange={e => setRejectReason(e.target.value)}
                placeholder="Sebep yazın..."
                style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', boxSizing: 'border-box' as any }}
              />
            </div>

            <div style={{ display: 'flex', gap: '10px' }}>
              <button onClick={() => setSelectedTransfer(null)}
                style={{ flex: 1, padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>
                Geri
              </button>
              <button onClick={handleReject} disabled={!rejectReason}
                style={{ flex: 1, padding: '10px', background: !rejectReason ? '#ccc' : '#ff4d4f', color: 'white', border: 'none', borderRadius: '8px', cursor: !rejectReason ? 'not-allowed' : 'pointer' }}>
                Reddet
              </button>
              <button onClick={handleApprove} disabled={!scheduleDate}
                style={{ flex: 1, padding: '10px', background: !scheduleDate ? '#ccc' : '#52c41a', color: 'white', border: 'none', borderRadius: '8px', cursor: !scheduleDate ? 'not-allowed' : 'pointer' }}>
                Onayla
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}