import { useState, useEffect, useCallback } from 'react';
import { getNotifications, markAsRead } from '../services/api';
import { Notification } from '../types';

export default function Notifications() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const user = JSON.parse(localStorage.getItem('user') || '{}');

  // user her render'da yeniden parse edildigi icin bagimlilik olarak
  // branchId kullanilir; aksi halde useEffect her render'da tetiklenir.
  const branchId = user.branchId;

  const fetchNotifications = useCallback(async () => {
    try {
      const res = await getNotifications(branchId);
      setNotifications(res.data.kayitlar);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [branchId]);

  useEffect(() => {
    fetchNotifications();
  }, [fetchNotifications]);

  const handleMarkAsRead = async (id: number) => {
    try {
      await markAsRead(id);
      setNotifications(prev =>
        prev.map(n => n.id === id ? { ...n, isRead: true } : n)
      );
    } catch (err) {
      console.error(err);
    }
  };

  const handleMarkAllRead = async () => {
    const unread = notifications.filter(n => !n.isRead);
    await Promise.all(unread.map(n => markAsRead(n.id)));
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div>
      {/* Üst bar */}
      <div style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        marginBottom: '20px'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <span style={{ fontSize: '16px', fontWeight: 600 }}>
            Tüm Bildirimler
          </span>
          {unreadCount > 0 && (
            <span style={{
              background: '#ff4d4f', color: 'white',
              padding: '2px 8px', borderRadius: '10px', fontSize: '12px', fontWeight: 600
            }}>
              {unreadCount} okunmamış
            </span>
          )}
        </div>
        {unreadCount > 0 && (
          <button
            onClick={handleMarkAllRead}
            style={{
              padding: '8px 16px', background: 'white', border: '1px solid #ddd',
              borderRadius: '8px', cursor: 'pointer', fontSize: '14px', color: '#1890ff'
            }}
          >
            Tümünü Okundu İşaretle
          </button>
        )}
      </div>

      {/* Bildirim listesi */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
        {notifications.length === 0 && (
          <div style={{
            background: 'white', borderRadius: '12px', padding: '40px',
            textAlign: 'center', color: '#999', boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
          }}>
            Bildirim yok.
          </div>
        )}
        {notifications.map(n => (
          <div key={n.id} style={{
            background: 'white', borderRadius: '12px', padding: '16px 20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
            borderLeft: `4px solid ${n.isRead ? '#f0f0f0' : '#1890ff'}`,
            display: 'flex', alignItems: 'center', justifyContent: 'space-between',
            opacity: n.isRead ? 0.7 : 1
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
              <div style={{ fontSize: '24px' }}>
                {n.transferRequestId ? '🔄' : '📦'}
              </div>
              <div>
                <div style={{ fontWeight: n.isRead ? 400 : 600, fontSize: '14px', marginBottom: '4px' }}>
                  {n.message}
                </div>
                <div style={{ fontSize: '12px', color: '#999' }}>
                  {new Date(n.createdAt).toLocaleString('tr-TR')}
                  {n.shipmentId && <span style={{ marginLeft: '8px' }}>• Kargo #{n.shipmentId}</span>}
                  {n.transferRequestId && <span style={{ marginLeft: '8px' }}>• Transfer #{n.transferRequestId}</span>}
                </div>
              </div>
            </div>
            {!n.isRead && (
              <button
                onClick={() => handleMarkAsRead(n.id)}
                style={{
                  padding: '6px 12px', background: '#e6f7ff', color: '#1890ff',
                  border: '1px solid #91d5ff', borderRadius: '6px',
                  cursor: 'pointer', fontSize: '12px', whiteSpace: 'nowrap'
                }}
              >
                Okundu
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}