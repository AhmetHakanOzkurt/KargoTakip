import React from 'react';

console.log('Layout loaded');

interface LayoutProps {
  user: any;
  onLogout: () => void;
  children: React.ReactNode;
  activePage: string;
  onNavigate: (page: string) => void;
}

const getMenuItems = (role: string) => {
  const allItems = [
    { key: 'dashboard', label: 'Dashboard', icon: '📊', roles: ['Admin', 'BranchManager'] },
    { key: 'create-order', label: 'Kargo Oluştur', icon: '📦', roles: ['Admin', 'BranchManager', 'Staff'] },
    { key: 'orders', label: 'Kargo Listesi', icon: '📋', roles: ['Admin', 'BranchManager', 'Staff'] },
    { key: 'vehicles', label: 'Araçlar', icon: '🚛', roles: ['Admin', 'BranchManager'] },
    { key: 'branches', label: 'Şubeler', icon: '🏢', roles: ['Admin', 'BranchManager'] },
    { key: 'consolidation', label: 'Konsolidasyon', icon: '🔄', roles: ['Admin'] },
    { key: 'notifications', label: 'Bildirimler', icon: '🔔', roles: ['Admin', 'BranchManager', 'Staff'] },
    { key: 'reports', label: 'Raporlar', icon: '📈', roles: ['Admin', 'BranchManager'] },
  ];
  return allItems.filter(item => item.roles.includes(role));
};

export default function Layout({ user, onLogout, children, activePage, onNavigate }: LayoutProps) {
  const [collapsed, setCollapsed] = React.useState(false);
  const menuItems = getMenuItems(user.role);
  console.log('Role:', user.role, 'Menu:', menuItems);

  return (
    <div style={{ display: 'flex', minHeight: '100vh', background: '#f0f2f5' }}>
      {/* Sidebar */}
      <div style={{
        width: collapsed ? '64px' : '240px',
        background: '#1a1a2e',
        transition: 'width 0.2s',
        display: 'flex',
        flexDirection: 'column',
        flexShrink: 0
      }}>
        {/* Logo */}
        <div style={{
          padding: '20px 16px',
          borderBottom: '1px solid rgba(255,255,255,0.1)',
          display: 'flex',
          alignItems: 'center',
          gap: '12px'
        }}>
          <span style={{ fontSize: '24px' }}>🚚</span>
          {!collapsed && (
            <span style={{ color: 'white', fontWeight: 700, fontSize: '16px' }}>KargoTakip</span>
          )}
          <button
            onClick={() => setCollapsed(!collapsed)}
            style={{
              marginLeft: 'auto',
              background: 'none',
              border: 'none',
              color: 'rgba(255,255,255,0.6)',
              cursor: 'pointer',
              fontSize: '18px'
            }}
          >
            {collapsed ? '→' : '←'}
          </button>
        </div>

        {/* Menu */}
        <nav style={{ flex: 1, padding: '12px 0', overflowY: 'auto' }}>
          {menuItems.map(item => (
            <button
              key={item.key}
              onClick={() => onNavigate(item.key)}
              style={{
                width: '100%',
                padding: '12px 16px',
                display: 'flex',
                alignItems: 'center',
                gap: '12px',
                background: activePage === item.key ? 'rgba(24,144,255,0.2)' : 'none',
                border: 'none',
                borderLeft: activePage === item.key ? '3px solid #1890ff' : '3px solid transparent',
                color: activePage === item.key ? '#1890ff' : 'rgba(255,255,255,0.7)',
                cursor: 'pointer',
                fontSize: '14px',
                textAlign: 'left'
              }}
            >
              <span style={{ fontSize: '18px', flexShrink: 0 }}>{item.icon}</span>
              {!collapsed && <span>{item.label}</span>}
            </button>
          ))}
        </nav>

        {/* User info */}
        <div style={{
          padding: '16px',
          borderTop: '1px solid rgba(255,255,255,0.1)'
        }}>
          {!collapsed && (
            <div style={{ color: 'rgba(255,255,255,0.7)', fontSize: '12px', marginBottom: '8px' }}>
              <div style={{ fontWeight: 600, color: 'white', marginBottom: '2px' }}>{user.fullName}</div>
              <div>{user.role} • {user.branchName}</div>
            </div>
          )}
          <button
            onClick={onLogout}
            style={{
              width: '100%',
              padding: '8px',
              background: 'rgba(255,77,79,0.2)',
              border: '1px solid rgba(255,77,79,0.4)',
              borderRadius: '6px',
              color: '#ff4d4f',
              cursor: 'pointer',
              fontSize: '13px'
            }}
          >
            {collapsed ? '↩' : 'Çıkış Yap'}
          </button>
        </div>
      </div>

      {/* Main content */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
        {/* Header */}
        <div style={{
          background: 'white',
          padding: '16px 24px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexShrink: 0
        }}>
          <h2 style={{ margin: 0, color: '#1a1a2e' }}>
            {menuItems.find(m => m.key === activePage)?.icon}{' '}
            {menuItems.find(m => m.key === activePage)?.label}
          </h2>
          <div style={{ color: '#666', fontSize: '14px' }}>
            {new Date().toLocaleDateString('tr-TR', {
              weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
            })}
          </div>
        </div>

        {/* Page content */}
        <div style={{ flex: 1, padding: '24px', overflowY: 'auto' }}>
          {children}
        </div>
      </div>
    </div>
  );
}