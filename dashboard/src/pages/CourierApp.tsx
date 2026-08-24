import { useState } from 'react';
import axios from 'axios';

// Sabit localhost portlari sunucuda/telefonda calismiyordu; api.ts ile ayni
// BASE kullanilir (nginx ayni origin uzerinden proxy'ler).
const BASE = process.env.REACT_APP_API_BASE || '';
const ORDER_URL = BASE;
const AUTH_URL = BASE;

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

  const fetchShipments = async (t: string) => {
    const res = await axios.get(`${ORDER_URL}/api/orders`, {
      headers: { Authorization: `Bearer ${t}` }
    });
    const filtered = res.data.filter((s: any) =>
      s.currentStatus === 'Dağıtımda' || s.currentStatus === 'Yolda'
    );
    setShipments(filtered);
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

  // Login ekranı
  if (step === 'login') {
    return (
      <div style={{
        minHeight: '100vh', background: '#1a1a2e',
        display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '20px'
      }}>
        <div style={{
          background: 'white', borderRadius: '16px', padding: '32px',
          width: '100%', maxWidth: '360px'
        }}>
          <div style={{ textAlign: 'center', marginBottom: '32px' }}>
            <div style={{ fontSize: '48px', marginBottom: '8px' }}>🚚</div>
            <h1 style={{ margin: 0, fontSize: '22px', color: '#1a1a2e' }}>Kurye Girişi</h1>
            <p style={{ color: '#999', fontSize: '14px', margin: '8px 0 0' }}>
              KargoTakip Teslimat Sistemi
            </p>
          </div>
          <form onSubmit={handleLogin}>
            <div style={{ marginBottom: '16px' }}>
              <input
                value={username}
                onChange={e => setUsername(e.target.value)}
                placeholder="Kullanıcı adı"
                style={{
                  width: '100%', padding: '14px', border: '1px solid #ddd',
                  borderRadius: '10px', fontSize: '16px', boxSizing: 'border-box' as any
                }}
              />
            </div>
            <div style={{ marginBottom: '24px' }}>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                placeholder="Şifre"
                style={{
                  width: '100%', padding: '14px', border: '1px solid #ddd',
                  borderRadius: '10px', fontSize: '16px', boxSizing: 'border-box' as any
                }}
              />
            </div>
            {error && (
              <div style={{
                background: '#fff2f0', border: '1px solid #ffccc7', borderRadius: '8px',
                padding: '10px', marginBottom: '16px', color: '#ff4d4f', fontSize: '14px'
              }}>
                {error}
              </div>
            )}
            <button type="submit" disabled={loading} style={{
              width: '100%', padding: '14px',
              background: loading ? '#ccc' : '#1890ff',
              color: 'white', border: 'none', borderRadius: '10px',
              fontSize: '16px', fontWeight: 600,
              cursor: loading ? 'not-allowed' : 'pointer'
            }}>
              {loading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
            </button>
          </form>
        </div>
      </div>
    );
  }

  // Teslimat listesi
  if (step === 'list') {
    return (
      <div style={{ minHeight: '100vh', background: '#f0f2f5' }}>
        <div style={{
          background: '#1a1a2e', padding: '20px',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between'
        }}>
          <div>
            <div style={{ color: 'white', fontWeight: 700, fontSize: '18px' }}>
              🚚 Teslimatlarım
            </div>
            <div style={{ color: 'rgba(255,255,255,0.6)', fontSize: '13px' }}>
              {user?.fullName}
            </div>
          </div>
          <button
            onClick={() => { setStep('login'); setToken(''); setUser(null); }}
            style={{
              background: 'rgba(255,255,255,0.1)', border: 'none',
              color: 'white', padding: '8px 16px', borderRadius: '8px', cursor: 'pointer'
            }}
          >
            Çıkış
          </button>
        </div>

        <div style={{ padding: '16px' }}>
          <div style={{ display: 'flex', gap: '12px', marginBottom: '16px' }}>
            <div style={{
              flex: 1, background: '#fa8c16', borderRadius: '12px', padding: '16px',
              color: 'white', textAlign: 'center'
            }}>
              <div style={{ fontSize: '28px', fontWeight: 700 }}>
                {shipments.filter(s => s.currentStatus === 'Dağıtımda').length}
              </div>
              <div style={{ fontSize: '13px', opacity: 0.9 }}>Dağıtımda</div>
            </div>
            <div style={{
              flex: 1, background: '#1890ff', borderRadius: '12px', padding: '16px',
              color: 'white', textAlign: 'center'
            }}>
              <div style={{ fontSize: '28px', fontWeight: 700 }}>
                {shipments.filter(s => s.currentStatus === 'Yolda').length}
              </div>
              <div style={{ fontSize: '13px', opacity: 0.9 }}>Yolda</div>
            </div>
          </div>

          {shipments.length === 0 ? (
            <div style={{
              background: 'white', borderRadius: '12px', padding: '40px',
              textAlign: 'center', color: '#999'
            }}>
              <div style={{ fontSize: '48px', marginBottom: '12px' }}>✅</div>
              <div>Tüm teslimatlar tamamlandı!</div>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              {shipments.map(s => (
                <div
                  key={s.id}
                  onClick={() => {
                    setSelected(s);
                    setStep('detail');
                    setError('');
                    setDeliveryCode('');
                  }}
                  style={{
                    background: 'white', borderRadius: '12px', padding: '16px',
                    cursor: 'pointer', boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                    borderLeft: `4px solid ${s.currentStatus === 'Dağıtımda' ? '#fa8c16' : '#1890ff'}`
                  }}
                >
                  <div style={{
                    display: 'flex', justifyContent: 'space-between', marginBottom: '8px'
                  }}>
                    <span style={{
                      fontFamily: 'monospace', fontWeight: 700,
                      color: '#1890ff', fontSize: '15px'
                    }}>
                      {s.trackingCode}
                    </span>
                    <span style={{
                      padding: '3px 10px', borderRadius: '10px', fontSize: '12px',
                      background: s.currentStatus === 'Dağıtımda' ? '#fff7e6' : '#e6f7ff',
                      color: s.currentStatus === 'Dağıtımda' ? '#fa8c16' : '#1890ff'
                    }}>
                      {s.currentStatus}
                    </span>
                  </div>
                  <div style={{ fontWeight: 600, marginBottom: '4px' }}>{s.receiverName}</div>
                  <div style={{ color: '#666', fontSize: '14px' }}>{s.receiverAddress}</div>
                  <div style={{ color: '#999', fontSize: '13px', marginTop: '4px' }}>
                    📍 {s.receiverCity}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    );
  }

  // Teslimat detayı
  return (
    <div style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <div style={{
        background: '#1a1a2e', padding: '20px',
        display: 'flex', alignItems: 'center', gap: '16px'
      }}>
        <button
          onClick={() => {
            setStep('list');
            setSelected(null);
            setError('');
            setDeliveryCode('');
          }}
          style={{
            background: 'none', border: 'none', color: 'white',
            fontSize: '24px', cursor: 'pointer'
          }}
        >
          ←
        </button>
        <div style={{ color: 'white', fontWeight: 700, fontSize: '18px' }}>
          Teslimat Detayı
        </div>
      </div>

      <div style={{ padding: '16px' }}>
        {/* Kargo bilgileri */}
        <div style={{
          background: 'white', borderRadius: '12px', padding: '20px',
          marginBottom: '16px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
        }}>
          <div style={{
            fontFamily: 'monospace', color: '#1890ff', fontWeight: 700,
            fontSize: '16px', marginBottom: '16px'
          }}>
            {selected?.trackingCode}
          </div>

          {[
            { label: '👤 Alıcı', value: selected?.receiverName },
            { label: '📍 Adres', value: selected?.receiverAddress },
            { label: '🏙️ Şehir', value: selected?.receiverCity },
            { label: '⚖️ Ağırlık', value: `${selected?.weight} kg` },
            { label: '📦 Durum', value: selected?.currentStatus },
          ].map(item => (
            <div key={item.label} style={{
              display: 'flex', justifyContent: 'space-between',
              padding: '10px 0', borderBottom: '1px solid #f0f0f0'
            }}>
              <span style={{ color: '#666', fontSize: '14px' }}>{item.label}</span>
              <span style={{
                fontWeight: 500, fontSize: '14px',
                maxWidth: '200px', textAlign: 'right'
              }}>
                {item.value}
              </span>
            </div>
          ))}
        </div>

        {/* Teslimat kodu - sadece Dağıtımda durumunda */}
        {selected?.currentStatus === 'Dağıtımda' ? (
          <div style={{
            background: 'white', borderRadius: '12px', padding: '20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
          }}>
            <h3 style={{ marginBottom: '8px', fontSize: '16px' }}>🔐 Teslimat Doğrulama</h3>
            <p style={{ color: '#666', fontSize: '14px', marginBottom: '16px' }}>
              Müşterinin e-postasına gönderilen 6 haneli kodu girin.
            </p>

            <input
              value={deliveryCode}
              onChange={e => setDeliveryCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              placeholder="_ _ _ _ _ _"
              maxLength={6}
              style={{
                width: '100%', padding: '16px', border: '2px solid #ddd',
                borderRadius: '10px', fontSize: '28px', fontWeight: 700,
                textAlign: 'center', letterSpacing: '8px',
                boxSizing: 'border-box' as any, marginBottom: '16px'
              }}
            />

            {error && (
              <div style={{
                background: '#fff2f0', border: '1px solid #ffccc7', borderRadius: '8px',
                padding: '10px', marginBottom: '16px', color: '#ff4d4f', fontSize: '14px'
              }}>
                {error}
              </div>
            )}

            {success && (
              <div style={{
                background: '#f6ffed', border: '1px solid #b7eb8f', borderRadius: '8px',
                padding: '16px', marginBottom: '16px', color: '#52c41a',
                fontSize: '16px', fontWeight: 600, textAlign: 'center'
              }}>
                {success}
              </div>
            )}

            <button
              onClick={handleDeliver}
              disabled={loading || deliveryCode.length !== 6}
              style={{
                width: '100%', padding: '16px',
                background: loading || deliveryCode.length !== 6 ? '#ccc' : '#52c41a',
                color: 'white', border: 'none', borderRadius: '10px',
                fontSize: '16px', fontWeight: 700,
                cursor: loading || deliveryCode.length !== 6 ? 'not-allowed' : 'pointer'
              }}
            >
              {loading ? '⏳ İşleniyor...' : '✅ Teslim Et'}
            </button>
          </div>
        ) : (
          <div style={{
            background: 'white', borderRadius: '12px', padding: '24px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.06)', textAlign: 'center'
          }}>
            <div style={{ fontSize: '48px', marginBottom: '12px' }}>🚚</div>
            <div style={{ fontWeight: 600, color: '#1890ff', marginBottom: '8px' }}>
              Kargo Yolda
            </div>
            <div style={{ color: '#666', fontSize: '14px' }}>
              Teslimat kodu kargo dağıtıma çıktığında müşteriye gönderilecek.
            </div>
          </div>
        )}
      </div>
    </div>
  );
}