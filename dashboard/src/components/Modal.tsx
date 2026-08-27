import { ReactNode } from 'react';

/**
 * Modal kaplamasi Orders sayfasinda bes kez birebir tekrarlaniyordu.
 * Tek yerde toplandi.
 */
export default function Modal({
  width,
  zIndex = 1000,
  scrollable = false,
  children
}: {
  width: string;
  zIndex?: number;
  scrollable?: boolean;
  children: ReactNode;
}) {
  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex
    }}>
      <div style={{
        background: 'white', borderRadius: '12px', padding: '32px', width,
        boxShadow: '0 8px 32px rgba(0,0,0,0.2)',
        ...(scrollable ? { maxHeight: '80vh', overflowY: 'auto' as const } : {})
      }}>
        {children}
      </div>
    </div>
  );
}
