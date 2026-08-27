import Modal from '../Modal';
import { Order } from '../../types';
import { KARGO_DURUMLARI } from './statusStyles';

export default function UpdateStatusModal({
  order,
  newStatus,
  updating,
  onStatusChange,
  onCancel,
  onConfirm
}: {
  order: Order;
  newStatus: string;
  updating: boolean;
  onStatusChange: (s: string) => void;
  onCancel: () => void;
  onConfirm: () => void;
}) {
  const devreDisi = !newStatus || updating;

  return (
    <Modal width="400px">
      <h3 style={{ marginBottom: '8px' }}>Durum Güncelle</h3>
      <p style={{ color: '#666', marginBottom: '20px', fontSize: '14px' }}>
        {order.trackingCode} — {order.currentStatus}
      </p>
      <select
        value={newStatus}
        onChange={e => onStatusChange(e.target.value)}
        style={{ width: '100%', padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', marginBottom: '16px', boxSizing: 'border-box' }}
      >
        <option value="">Yeni durum seç...</option>
        {KARGO_DURUMLARI.map(d => <option key={d} value={d}>{d}</option>)}
      </select>
      <div style={{ display: 'flex', gap: '12px' }}>
        <button onClick={onCancel}
          style={{ flex: 1, padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>
          İptal
        </button>
        <button onClick={onConfirm} disabled={devreDisi}
          style={{ flex: 1, padding: '10px', background: devreDisi ? '#ccc' : '#1890ff', color: 'white', border: 'none', borderRadius: '8px', cursor: devreDisi ? 'not-allowed' : 'pointer' }}>
          {updating ? 'Güncelleniyor...' : 'Güncelle'}
        </button>
      </div>
    </Modal>
  );
}
