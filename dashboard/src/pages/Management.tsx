import { useState, useEffect } from 'react';
import { createUser, createVehicle, getBranches, getCities, getVehicleTypes } from '../services/api';

export default function Management() {
  const [activeTab, setActiveTab] = useState<'user' | 'vehicle'>('user');
  const [branches, setBranches] = useState<any[]>([]);
  const [cities, setCities] = useState<any[]>([]);
  const [vehicleTypes, setVehicleTypes] = useState<any[]>([]);
  const [msg, setMsg] = useState('');

  // Kullanıcı form
  const [uForm, setUForm] = useState({ username: '', password: '', fullName: '', role: 'Staff', branchId: '' });
  const [uLoading, setULoading] = useState(false);

  // Araç form
  const [vForm, setVForm] = useState({ plateNumber: '', capacity: '', vehicleTypeId: '', branchId: '', cityId: '' });
  const [vLoading, setVLoading] = useState(false);

  useEffect(() => {
    getBranches().then(r => setBranches(r.data)).catch(() => {});
    getCities().then(r => setCities(r.data)).catch(() => {});
    getVehicleTypes().then(r => setVehicleTypes(r.data)).catch(() => {});
  }, []);

  const handleCreateUser = async () => {
    if (!uForm.username || !uForm.password || !uForm.fullName || !uForm.branchId) {
      setMsg('❌ Tüm alanları doldurun.');
      return;
    }
    setULoading(true);
    setMsg('');
    try {
      await createUser({ ...uForm, branchId: parseInt(uForm.branchId) });
      setMsg('✅ Kullanıcı başarıyla oluşturuldu.');
      setUForm({ username: '', password: '', fullName: '', role: 'Staff', branchId: '' });
    } catch (err: any) {
      setMsg('❌ ' + (err.response?.data?.message || 'Kullanıcı oluşturulamadı.'));
    } finally {
      setULoading(false);
    }
  };

  const handleCreateVehicle = async () => {
    if (!vForm.plateNumber || !vForm.capacity || !vForm.vehicleTypeId || !vForm.branchId || !vForm.cityId) {
      setMsg('❌ Tüm alanları doldurun.');
      return;
    }
    setVLoading(true);
    setMsg('');
    try {
      await createVehicle({
        plateNumber: vForm.plateNumber,
        capacity: parseInt(vForm.capacity),
        vehicleTypeId: parseInt(vForm.vehicleTypeId),
        branchId: parseInt(vForm.branchId),
        cityId: parseInt(vForm.cityId)
      });
      setMsg('✅ Araç başarıyla eklendi.');
      setVForm({ plateNumber: '', capacity: '', vehicleTypeId: '', branchId: '', cityId: '' });
    } catch (err: any) {
      setMsg('❌ ' + (err.response?.data?.message || 'Araç eklenemedi.'));
    } finally {
      setVLoading(false);
    }
  };

  const inputStyle: React.CSSProperties = {
    width: '100%', padding: '10px 12px', border: '1px solid #ddd',
    borderRadius: '8px', fontSize: '14px', boxSizing: 'border-box', marginTop: '6px'
  };
  const labelStyle: React.CSSProperties = { fontSize: '13px', color: '#555', fontWeight: 600, display: 'block', marginBottom: '14px' };

  return (
    <div style={{ maxWidth: '560px' }}>
      {/* Tab */}
      <div style={{ display: 'flex', gap: '8px', marginBottom: '24px' }}>
        {[{ key: 'user', label: '👤 Kullanıcı Ekle' }, { key: 'vehicle', label: '🚛 Araç Ekle' }].map(t => (
          <button key={t.key} onClick={() => { setActiveTab(t.key as any); setMsg(''); }}
            style={{
              padding: '10px 20px', background: activeTab === t.key ? '#1890ff' : 'white',
              color: activeTab === t.key ? 'white' : '#666', border: '1px solid',
              borderColor: activeTab === t.key ? '#1890ff' : '#ddd', borderRadius: '8px',
              cursor: 'pointer', fontWeight: activeTab === t.key ? 600 : 400
            }}>{t.label}</button>
        ))}
      </div>

      <div style={{ background: 'white', borderRadius: '12px', padding: '28px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}>
        {msg && (
          <div style={{ padding: '10px 14px', borderRadius: '8px', marginBottom: '20px', fontSize: '13px',
            background: msg.startsWith('✅') ? '#f6ffed' : '#fff1f0',
            color: msg.startsWith('✅') ? '#52c41a' : '#ff4d4f' }}>
            {msg}
          </div>
        )}

        {activeTab === 'user' && (
          <div>
            <label style={labelStyle}>
              Ad Soyad
              <input style={inputStyle} value={uForm.fullName} onChange={e => setUForm(p => ({ ...p, fullName: e.target.value }))} placeholder="Ahmet Yılmaz" />
            </label>
            <label style={labelStyle}>
              Kullanıcı Adı
              <input style={inputStyle} value={uForm.username} onChange={e => setUForm(p => ({ ...p, username: e.target.value }))} placeholder="ahmet_yilmaz" />
            </label>
            <label style={labelStyle}>
              Şifre
              <input style={inputStyle} type="password" value={uForm.password} onChange={e => setUForm(p => ({ ...p, password: e.target.value }))} placeholder="••••••••" />
            </label>
            <label style={labelStyle}>
              Rol
              <select style={inputStyle} value={uForm.role} onChange={e => setUForm(p => ({ ...p, role: e.target.value }))}>
                <option value="Staff">Staff (Personel)</option>
                <option value="BranchManager">BranchManager (Şube Müdürü)</option>
                <option value="Admin">Admin</option>
              </select>
            </label>
            <label style={labelStyle}>
              Şube
              <select style={inputStyle} value={uForm.branchId} onChange={e => setUForm(p => ({ ...p, branchId: e.target.value }))}>
                <option value="">Şube seç...</option>
                {branches.map((b: any) => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </label>
            <button onClick={handleCreateUser} disabled={uLoading}
              style={{ width: '100%', padding: '12px', background: uLoading ? '#ccc' : '#1890ff', color: 'white', border: 'none', borderRadius: '8px', cursor: uLoading ? 'not-allowed' : 'pointer', fontSize: '15px', fontWeight: 600 }}>
              {uLoading ? 'Oluşturuluyor...' : 'Kullanıcı Oluştur'}
            </button>
          </div>
        )}

        {activeTab === 'vehicle' && (
          <div>
            <label style={labelStyle}>
              Plaka
              <input style={inputStyle} value={vForm.plateNumber} onChange={e => setVForm(p => ({ ...p, plateNumber: e.target.value }))} placeholder="34 ABC 001" />
            </label>
            <label style={labelStyle}>
              Kapasite (adet)
              <input style={inputStyle} type="number" value={vForm.capacity} onChange={e => setVForm(p => ({ ...p, capacity: e.target.value }))} placeholder="20" />
            </label>
            <label style={labelStyle}>
              Araç Tipi
              <select style={inputStyle} value={vForm.vehicleTypeId} onChange={e => setVForm(p => ({ ...p, vehicleTypeId: e.target.value }))}>
                <option value="">Tip seç...</option>
                {vehicleTypes.map((t: any) => <option key={t.id} value={t.id}>{t.name} ({t.routeType})</option>)}
              </select>
            </label>
            <label style={labelStyle}>
              Şube
              <select style={inputStyle} value={vForm.branchId} onChange={e => setVForm(p => ({ ...p, branchId: e.target.value }))}>
                <option value="">Şube seç...</option>
                {branches.map((b: any) => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </label>
            <label style={labelStyle}>
              Bulunduğu Şehir
              <select style={inputStyle} value={vForm.cityId} onChange={e => setVForm(p => ({ ...p, cityId: e.target.value }))}>
                <option value="">Şehir seç...</option>
                {cities.map((c: any) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </label>
            <button onClick={handleCreateVehicle} disabled={vLoading}
              style={{ width: '100%', padding: '12px', background: vLoading ? '#ccc' : '#1890ff', color: 'white', border: 'none', borderRadius: '8px', cursor: vLoading ? 'not-allowed' : 'pointer', fontSize: '15px', fontWeight: 600 }}>
              {vLoading ? 'Ekleniyor...' : 'Araç Ekle'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}