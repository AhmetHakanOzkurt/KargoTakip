import { useState } from 'react';
import axios from 'axios';
import { CourierLogin, CourierList, CourierDetail } from '../components/courier/CourierScreens';

// Sabit localhost portlari sunucuda/telefonda calismiyordu; api.ts ile ayni
// BASE kullanilir (nginx ayni origin uzerinden proxy'ler).
const BASE = process.env.REACT_APP_API_BASE || '';
const ORDER_URL = BASE;
const AUTH_URL = BASE;

const KURYE_DURUMLARI = ['Dağıtımda', 'Yolda'];

export default function CourierApp() {
  const [step, setStep] = useState<'login' | 'list' | 'detail'>('login');
  const [token, setToken] = useState('');
  const [user, setUser] = useState<any>(null);
  const [shipments, setShipments] = useState<any[]>([]);
  const [selected, setSelected] = useState<any>(null);
  const [deliveryCode, setDeliveryCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const fetchShipments = async (t: string) => {
    const res = await axios.get(`${ORDER_URL}/api/orders`, {
      headers: { Authorization: `Bearer ${t}` }
    });

    // /api/orders sayfalama zarfi dondurur: { toplamKayit, sayfa, ..., kayitlar }
    setShipments(
      res.data.kayitlar.filter((s: any) => KURYE_DURUMLARI.includes(s.currentStatus))
    );
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const res = await axios.post(`${AUTH_URL}/api/auth/login`, { username, password });
      setToken(res.data.token);
      setUser(res.data);
      await fetchShipments(res.data.token);
      setStep('list');
    } catch {
      setError('Kullanıcı adı veya şifre hatalı.');
    } finally {
      setLoading(false);
    }
  };

  const handleDeliver = async () => {
    if (!deliveryCode) {
      setError('Teslimat kodu giriniz.');
      return;
    }
    setLoading(true);
    setError('');
    try {
      await axios.put(
        `${ORDER_URL}/api/orders/${selected.id}/deliver`,
        { deliveryCode },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setSuccess('✅ Kargo başarıyla teslim edildi!');
      setDeliveryCode('');
      setTimeout(async () => {
        setSuccess('');
        setStep('list');
        setSelected(null);
        await fetchShipments(token);
      }, 2000);
    } catch (err: any) {
      setError(err.response?.data?.message ?? 'Teslimat kodu hatalı veya geçersiz.');
    } finally {
      setLoading(false);
    }
  };

  const listeyeDon = () => {
    setStep('list');
    setSelected(null);
    setError('');
    setDeliveryCode('');
  };

  if (step === 'login') {
    return (
      <CourierLogin
        username={username}
        password={password}
        loading={loading}
        error={error}
        onUsernameChange={setUsername}
        onPasswordChange={setPassword}
        onSubmit={handleLogin}
      />
    );
  }

  if (step === 'list') {
    return (
      <CourierList
        shipments={shipments}
        fullName={user?.fullName}
        onSelect={s => {
          setSelected(s);
          setStep('detail');
          setError('');
          setDeliveryCode('');
        }}
        onLogout={() => { setStep('login'); setToken(''); setUser(null); }}
      />
    );
  }

  return (
    <CourierDetail
      shipment={selected}
      deliveryCode={deliveryCode}
      loading={loading}
      error={error}
      success={success}
      onCodeChange={setDeliveryCode}
      onDeliver={handleDeliver}
      onBack={listeyeDon}
    />
  );
}
