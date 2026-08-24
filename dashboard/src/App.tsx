import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Orders from './pages/Orders';
import Vehicles from './pages/Vehicles';
import Consolidation from './pages/Consolidation';
import Notifications from './pages/Notifications';
import Reports from './pages/Reports';
import Branches from './pages/Branches';
import CreateOrder from './pages/CreateOrder';
import CourierApp from './pages/CourierApp';
import TrackingPage from './pages/TrackingPage';
import Management from './pages/Management';

console.log('Current path:', window.location.pathname);

function MainApp() {
  const [user, setUser] = useState<any>(null);
  const [activePage, setActivePage] = useState('dashboard');

  useEffect(() => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) setUser(JSON.parse(savedUser));
  }, []);

  const handleLogin = (userData: any) => setUser(userData);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  if (!user) return <Login onLogin={handleLogin} />;

  const renderPage = () => {
    switch (activePage) {
      case 'dashboard': return <Dashboard />;
      case 'orders': return <Orders />;
      case 'vehicles': return <Vehicles />;
      case 'consolidation': return <Consolidation />;
      case 'notifications': return <Notifications />;
      case 'reports': return <Reports />;
      case 'branches': return <Branches />;
      case 'create-order': return <CreateOrder />;
      case 'management': return <Management />;
      default: return <div style={{ padding: '40px', color: '#666' }}>Bu sayfa yakında eklenecek...</div>;
    }
  };

  return (
    <Layout
      user={user}
      onLogout={handleLogout}
      activePage={activePage}
      onNavigate={setActivePage}
    >
      {renderPage()}
    </Layout>
  );
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/courier" element={<CourierApp />} />
        <Route path="/*" element={<MainApp />} />
        <Route path="/track" element={<TrackingPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;