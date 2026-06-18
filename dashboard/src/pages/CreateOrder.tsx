import { useState, useEffect } from 'react';
import { createOrder } from '../services/api';
import axios from 'axios';

export default function CreateOrder() {
  const [cities, setCities] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');
  const user = JSON.parse(localStorage.getItem('user') || '{}');

  const [form, setForm] = useState({
    senderName: '',
    senderPhone: '',
    receiverName: '',
    receiverPhone: '',
    receiverEmail: '',
    receiverAddress: '',
    receiverCityId: '',
    weight: '',
    priority: 'Normal',
    description: ''
  });

  useEffect(() => {
    // Şehirleri çek
    const fetchCities = async () => {
      try {
        const token = localStorage.getItem('token');
        const res = await axios.get('/api/orders/cities', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setCities(res.data);
      } catch {
        // Cities endpoint yoksa manuel liste kullan
        setCities([
    { id: 1, name: 'İstanbul' },
    { id: 2, name: 'Ankara' },
    { id: 3, name: 'Adana' },
    { id: 4, name: 'İzmir' },
    { id: 5, name: 'Bursa' },
    { id: 6, name: 'Mersin' },
    { id: 7, name: 'Antalya' },
    { id: 8, name: 'Konya' },
    { id: 9, name: 'Gaziantep' },
    { id: 10, name: 'Kayseri' },
    { id: 11, name: 'Samsun' },
    { id: 12, name: 'Trabzon' },
    { id: 13, name: 'Erzurum' },
    { id: 14, name: 'Diyarbakır' },
    { id: 15, name: 'Malatya' },
  ]);
      }
    };
    fetchCities();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');

    if (!form.senderName || !form.receiverName || !form.receiverAddress || !form.receiverCityId || !form.weight) {
      setError('Lütfen zorunlu alanları doldurun.');
      setLoading(false);
      return;
    }

    try {
      const res = await createOrder({
        senderName: form.senderName,
        receiverName: form.receiverName,
        receiverAddress: form.receiverAddress,
        receiverCityId: parseInt(form.receiverCityId),
        weight: parseFloat(form.weight),
        priority: form.priority,
        branchId: user.branchId,
        createdByUserId: user.userId || 1,
        receiverEmail: form.receiverEmail 
      });

      setSuccess(`✅ Kargo oluşturuldu! Takip kodu: ${res.data.trackingCode}`);
      setForm({
        senderName: '',
        senderPhone: '',
        receiverName: '',
        receiverPhone: '',
        receiverEmail: '',
        receiverAddress: '',
        receiverCityId: '',
        weight: '',
        priority: 'Normal',
        description: ''
      });
    } catch (err: any) {
      const errors = err.response?.data;
      if (Array.isArray(errors)) {
        setError(errors.map((e: any) => e.message).join(', '));
      } else {
        setError('Kargo oluşturulamadı. Lütfen tekrar deneyin.');
      }
    } finally {
      setLoading(false);
    }
  };

  const inputStyle = {
    width: '100%',
    padding: '10px 12px',
    border: '1px solid #ddd',
    borderRadius: '8px',
    fontSize: '14px',
    boxSizing: 'border-box' as any,
    outline: 'none'
  };

  const labelStyle = {
    display: 'block',
    marginBottom: '6px',
    fontWeight: 500,
    fontSize: '14px',
    color: '#333'
  };

  return (
    <div style={{ maxWidth: '800px', margin: '0 auto' }}>
      <div style={{
        background: 'white', borderRadius: '12px', padding: '32px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
      }}>
        <h2 style={{ marginBottom: '8px', color: '#1a1a2e' }}>📦 Yeni Kargo Oluştur</h2>
        <p style={{ color: '#666', marginBottom: '32px', fontSize: '14px' }}>
          Şube: <strong>{user.branchName}</strong>
        </p>

        <form onSubmit={handleSubmit}>
          {/* Gönderici Bilgileri */}
          <div style={{
            background: '#f8f9fa', borderRadius: '10px', padding: '20px', marginBottom: '20px'
          }}>
            <h3 style={{ marginBottom: '16px', fontSize: '15px', color: '#1890ff' }}>
              👤 Gönderici Bilgileri
            </h3>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
              <div>
                <label style={labelStyle}>Ad Soyad *</label>
                <input
                  name="senderName"
                  value={form.senderName}
                  onChange={handleChange}
                  placeholder="Ahmet Yılmaz"
                  style={inputStyle}
                />
              </div>
              <div>
                <label style={labelStyle}>Telefon</label>
                <input
                  name="senderPhone"
                  value={form.senderPhone}
                  onChange={handleChange}
                  placeholder="05XX XXX XX XX"
                  style={inputStyle}
                />
              </div>
              <div>
  <label style={labelStyle}>E-posta</label>
  <input
    name="receiverEmail"
    type="email"
    value={form.receiverEmail}
    onChange={handleChange}
    placeholder="ornek@email.com"
    style={inputStyle}
  />
</div>
            </div>
          </div>

          {/* Alıcı Bilgileri */}
          <div style={{
            background: '#f8f9fa', borderRadius: '10px', padding: '20px', marginBottom: '20px'
          }}>
            <h3 style={{ marginBottom: '16px', fontSize: '15px', color: '#52c41a' }}>
              📍 Alıcı Bilgileri
            </h3>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
              <div>
                <label style={labelStyle}>Ad Soyad *</label>
                <input
                  name="receiverName"
                  value={form.receiverName}
                  onChange={handleChange}
                  placeholder="Mehmet Demir"
                  style={inputStyle}
                />
              </div>
              <div>
                <label style={labelStyle}>Telefon</label>
                <input
                  name="receiverPhone"
                  value={form.receiverPhone}
                  onChange={handleChange}
                  placeholder="05XX XXX XX XX"
                  style={inputStyle}
                />
              </div>
            </div>
            <div style={{ marginBottom: '16px' }}>
              <label style={labelStyle}>Teslimat Adresi *</label>
              <textarea
                name="receiverAddress"
                value={form.receiverAddress}
                onChange={handleChange}
                placeholder="Mahalle, sokak, bina no, daire no..."
                rows={3}
                style={{ ...inputStyle, resize: 'vertical' }}
              />
            </div>
            <div>
              <label style={labelStyle}>Şehir *</label>
              <select
                name="receiverCityId"
                value={form.receiverCityId}
                onChange={handleChange}
                style={{ ...inputStyle, background: 'white' }}
              >
                <option value="">Şehir seçin...</option>
                {cities.map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Kargo Detayları */}
          <div style={{
            background: '#f8f9fa', borderRadius: '10px', padding: '20px', marginBottom: '24px'
          }}>
            <h3 style={{ marginBottom: '16px', fontSize: '15px', color: '#722ed1' }}>
              📋 Kargo Detayları
            </h3>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
              <div>
                <label style={labelStyle}>Ağırlık (kg) *</label>
                <input
                  name="weight"
                  type="number"
                  step="0.1"
                  min="0.1"
                  value={form.weight}
                  onChange={handleChange}
                  placeholder="0.0"
                  style={inputStyle}
                />
              </div>
              <div>
                <label style={labelStyle}>Öncelik</label>
                <select
                  name="priority"
                  value={form.priority}
                  onChange={handleChange}
                  style={{ ...inputStyle, background: 'white' }}
                >
                  <option value="Normal">Normal</option>
                  <option value="Acil">Acil</option>
                  <option value="Express">Express</option>
                </select>
              </div>
            </div>
            <div>
              <label style={labelStyle}>Açıklama</label>
              <textarea
                name="description"
                value={form.description}
                onChange={handleChange}
                placeholder="Kargo hakkında ek bilgi..."
                rows={2}
                style={{ ...inputStyle, resize: 'vertical' }}
              />
            </div>
          </div>

          {/* Hata ve başarı mesajları */}
          {error && (
            <div style={{
              background: '#fff2f0', border: '1px solid #ffccc7',
              borderRadius: '8px', padding: '12px 16px',
              marginBottom: '16px', color: '#ff4d4f', fontSize: '14px'
            }}>
              ❌ {error}
            </div>
          )}

          {success && (
            <div style={{
              background: '#f6ffed', border: '1px solid #b7eb8f',
              borderRadius: '8px', padding: '12px 16px',
              marginBottom: '16px', color: '#52c41a', fontSize: '14px',
              fontWeight: 500
            }}>
              {success}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            style={{
              width: '100%', padding: '14px',
              background: loading ? '#ccc' : '#1890ff',
              color: 'white', border: 'none', borderRadius: '8px',
              fontSize: '16px', fontWeight: 600,
              cursor: loading ? 'not-allowed' : 'pointer'
            }}
          >
            {loading ? '⏳ Oluşturuluyor...' : '📦 Kargoyu Oluştur'}
          </button>
        </form>
      </div>
    </div>
  );
}