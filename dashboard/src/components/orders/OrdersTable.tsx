import { Order } from '../../types';
import { statusColors, oncelikRengi } from './statusStyles';

const BASLIKLAR = [
  'Takip Kodu', 'Gönderici', 'Alıcı', 'Hedef Şehir',
  'Ağırlık', 'Öncelik', 'Durum', 'Araç', 'İşlem'
];

const TERMINAL_DURUMLAR = ['Teslim Edildi', 'İptal'];

function OrderRow({ order, onUpdate }: { order: Order; onUpdate: (o: Order) => void }) {
  const oncelik = oncelikRengi(order.priority);

  return (
    <tr style={{ borderBottom: '1px solid #f0f0f0' }}
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
          background: oncelik.bg, color: oncelik.fg
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
        {!TERMINAL_DURUMLAR.includes(order.currentStatus) && (
          <button
            onClick={() => onUpdate(order)}
            style={{ padding: '6px 12px', background: '#1890ff', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontSize: '13px' }}
          >
            Güncelle
          </button>
        )}
      </td>
    </tr>
  );
}

export default function OrdersTable({
  orders,
  onUpdate
}: {
  orders: Order[];
  onUpdate: (o: Order) => void;
}) {
  return (
    <div style={{ background: 'white', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', overflow: 'hidden' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#fafafa', borderBottom: '2px solid #f0f0f0' }}>
            {BASLIKLAR.map(h => (
              <th key={h} style={{ padding: '12px 16px', textAlign: 'left', color: '#666', fontWeight: 600, fontSize: '13px' }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {orders.map(order => (
            <OrderRow key={order.id} order={order} onUpdate={onUpdate} />
          ))}
        </tbody>
      </table>
      {orders.length === 0 && (
        <div style={{ padding: '40px', textAlign: 'center', color: '#999' }}>Kargo bulunamadı.</div>
      )}
    </div>
  );
}
