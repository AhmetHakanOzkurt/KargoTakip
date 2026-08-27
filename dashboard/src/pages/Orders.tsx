import { useState, useEffect, useCallback } from 'react';
import { getOrders, updateOrderStatus, createTransfer, getOutgoingTransfers, getIncomingTransfers, approveTransfer, rejectTransfer, getBranches } from '../services/api';
import { Order } from '../types';
import OrdersTable from '../components/orders/OrdersTable';
import UpdateStatusModal from '../components/orders/UpdateStatusModal';
import CreateTransferModal from '../components/orders/CreateTransferModal';
import { OutgoingTransfersModal, IncomingTransfersModal, RespondTransferModal } from '../components/orders/TransferListModals';
import { KARGO_DURUMLARI } from '../components/orders/statusStyles';

const hataMesaji = (err: any, varsayilan: string) =>
  '❌ ' + (err.response?.data?.message || varsayilan);

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

  const fetchOrders = useCallback(async () => {
    try {
      const res = await getOrders();
      setOrders(res.data.kayitlar);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchOrders();
    getBranches().then(r => setBranches(r.data)).catch(() => {});
  }, [fetchOrders]);

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
      setTransferMsg(hataMesaji(err, 'Transfer talebi oluşturulamadı.'));
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

  /** Onay ve red akisi ayni: istegi at, listeyi tazele, modali kapat. */
  const transferYanitla = async (
    istek: () => Promise<unknown>,
    basariMesaji: string,
    hataVarsayilani: string
  ) => {
    try {
      await istek();
      setTransferActionMsg(basariMesaji);
      const res = await getIncomingTransfers();
      setIncoming(res.data);
      setSelectedTransfer(null);
    } catch (err: any) {
      setTransferActionMsg(hataMesaji(err, hataVarsayilani));
    }
  };

  const handleApprove = () => {
    if (!selectedTransfer || !scheduleDate) return;
    transferYanitla(
      () => approveTransfer(selectedTransfer.id, { scheduledAt: new Date(scheduleDate).toISOString() }),
      '✅ Transfer talebi onaylandı.',
      'Onaylanamadı.'
    );
  };

  const handleReject = () => {
    if (!selectedTransfer || !rejectReason) return;
    transferYanitla(
      () => rejectTransfer(selectedTransfer.id, { reason: rejectReason }),
      '✅ Transfer talebi reddedildi.',
      'Reddedilemedi.'
    );
  };

  const handleToggleTransferOrder = (id: number, isaretli: boolean) => {
    setTransferOrders(prev =>
      isaretli ? [...prev, id] : prev.filter(x => x !== id));
  };

  const aramayaUyuyor = (o: Order) => {
    if (search === '') return true;
    const q = search.toLowerCase();
    return o.trackingCode.toLowerCase().includes(q) ||
      o.receiverName.toLowerCase().includes(q) ||
      o.senderName.toLowerCase().includes(q);
  };

  const filtered = orders.filter(o =>
    aramayaUyuyor(o) && (statusFilter === '' || o.currentStatus === statusFilter));

  const transferableOrders = filtered.filter(o => o.currentStatus === 'Hazırlanıyor');
  const gelenTalepleriGorebilir = user.role === 'BranchManager' || user.role === 'Admin';

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
          {KARGO_DURUMLARI.map(d => <option key={d} value={d}>{d}</option>)}
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
        {gelenTalepleriGorebilir && (
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

      <OrdersTable
        orders={filtered}
        onUpdate={order => { setSelectedOrder(order); setNewStatus(''); }}
      />

      {selectedOrder && (
        <UpdateStatusModal
          order={selectedOrder}
          newStatus={newStatus}
          updating={updating}
          onStatusChange={setNewStatus}
          onCancel={() => setSelectedOrder(null)}
          onConfirm={handleUpdateStatus}
        />
      )}

      {showTransferModal && (
        <CreateTransferModal
          branches={branches}
          userBranchId={user.branchId}
          transferableOrders={transferableOrders}
          targetBranchId={targetBranchId}
          selectedIds={transferOrders}
          note={transferNote}
          message={transferMsg}
          sending={transferring}
          onTargetChange={setTargetBranchId}
          onToggleOrder={handleToggleTransferOrder}
          onNoteChange={setTransferNote}
          onClose={() => { setShowTransferModal(false); setTransferMsg(''); setTransferOrders([]); }}
          onSubmit={handleTransfer}
        />
      )}

      {showOutgoing && (
        <OutgoingTransfersModal
          transfers={outgoing}
          onClose={() => setShowOutgoing(false)}
        />
      )}

      {showIncoming && (
        <IncomingTransfersModal
          transfers={incoming}
          message={transferActionMsg}
          onSelect={t => { setSelectedTransfer(t); setRejectReason(''); setScheduleDate(''); setTransferActionMsg(''); }}
          onClose={() => { setShowIncoming(false); setTransferActionMsg(''); }}
        />
      )}

      {selectedTransfer && (
        <RespondTransferModal
          transfer={selectedTransfer}
          scheduleDate={scheduleDate}
          rejectReason={rejectReason}
          onScheduleChange={setScheduleDate}
          onReasonChange={setRejectReason}
          onBack={() => setSelectedTransfer(null)}
          onReject={handleReject}
          onApprove={handleApprove}
        />
      )}
    </div>
  );
}
