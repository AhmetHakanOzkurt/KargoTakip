import Modal from '../Modal';
import { Order } from '../../types';
import { mesajRengi } from './statusStyles';

function KargoSecimi({
  orders,
  secili,
  onToggle
}: {
  orders: Order[];
  secili: number[];
  onToggle: (id: number, isaretli: boolean) => void;
}) {
  if (orders.length === 0) {
    return (
      <div style={{ padding: '16px', color: '#999', fontSize: '13px', textAlign: 'center' }}>
        Transfer edilebilir kargo yok.
      </div>
    );
  }

  return (
    <>
      {orders.map(o => (
        <label key={o.id} style={{ display: 'flex', alignItems: 'center', gap: '10px', padding: '10px 14px', borderBottom: '1px solid #f0f0f0', cursor: 'pointer' }}>
          <input
            type="checkbox"
            checked={secili.includes(o.id)}
            onChange={e => onToggle(o.id, e.target.checked)}
          />
          <span style={{ fontFamily: 'monospace', fontSize: '13px', color: '#1890ff' }}>{o.trackingCode}</span>
          <span style={{ fontSize: '13px', color: '#666' }}>{o.receiverName} — {o.receiverCity}</span>
        </label>
      ))}
    </>
  );
}

export default function CreateTransferModal({
  branches,
  userBranchId,
  transferableOrders,
  targetBranchId,
  selectedIds,
  note,
  message,
  sending,
  onTargetChange,
  onToggleOrder,
  onNoteChange,
  onClose,
  onSubmit
}: {
  branches: any[];
  userBranchId: number;
  transferableOrders: Order[];
  targetBranchId: string;
  selectedIds: number[];
  note: string;
  message: string;
  sending: boolean;
  onTargetChange: (v: string) => void;
  onToggleOrder: (id: number, isaretli: boolean) => void;
  onNoteChange: (v: string) => void;
  onClose: () => void;
  onSubmit: () => void;
}) {
  const devreDisi = !targetBranchId || selectedIds.length === 0 || sending;
  const mesajStil = mesajRengi(message);

  return (
    <Modal width="520px" scrollable>
      <h3 style={{ marginBottom: '4px' }}>🔄 Transfer Talebi Oluştur</h3>
      <p style={{ color: '#666', fontSize: '13px', marginBottom: '20px' }}>
        Sadece "Hazırlanıyor" durumundaki kargolar transfer edilebilir.
      </p>

      <div style={{ marginBottom: '16px' }}>
        <label style={{ fontSize: '13px', color: '#555', fontWeight: 600 }}>Hedef Şube</label>
        <select
          value={targetBranchId}
          onChange={e => onTargetChange(e.target.value)}
          style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', marginTop: '6px', boxSizing: 'border-box' }}
        >
          <option value="">Şube seç...</option>
          {branches.filter((b: any) => b.id !== userBranchId).map((b: any) => (
            <option key={b.id} value={b.id}>{b.name}</option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <label style={{ fontSize: '13px', color: '#555', fontWeight: 600 }}>Transfer Edilecek Kargolar</label>
        <div style={{ marginTop: '8px', maxHeight: '200px', overflowY: 'auto', border: '1px solid #eee', borderRadius: '8px' }}>
          <KargoSecimi orders={transferableOrders} secili={selectedIds} onToggle={onToggleOrder} />
        </div>
        {selectedIds.length > 0 && (
          <div style={{ fontSize: '12px', color: '#722ed1', marginTop: '6px' }}>
            {selectedIds.length} kargo seçildi
          </div>
        )}
      </div>

      <div style={{ marginBottom: '20px' }}>
        <label style={{ fontSize: '13px', color: '#555', fontWeight: 600 }}>Not (isteğe bağlı)</label>
        <textarea
          value={note}
          onChange={e => onNoteChange(e.target.value)}
          placeholder="Transfer sebebi veya notunuz..."
          rows={3}
          style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', marginTop: '6px', resize: 'none', boxSizing: 'border-box' }}
        />
      </div>

      {message && (
        <div style={{ padding: '10px 14px', borderRadius: '8px', marginBottom: '16px', background: mesajStil.bg, color: mesajStil.fg, fontSize: '13px' }}>
          {message}
        </div>
      )}

      <div style={{ display: 'flex', gap: '12px' }}>
        <button onClick={onClose}
          style={{ flex: 1, padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>
          Kapat
        </button>
        <button onClick={onSubmit} disabled={devreDisi}
          style={{ flex: 1, padding: '10px', background: devreDisi ? '#ccc' : '#722ed1', color: 'white', border: 'none', borderRadius: '8px', cursor: devreDisi ? 'not-allowed' : 'pointer' }}>
          {sending ? 'Gönderiliyor...' : 'Transfer Talebi Gönder'}
        </button>
      </div>
    </Modal>
  );
}
