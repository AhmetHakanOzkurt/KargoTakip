export function ErrorBox({ message }: { message: string }) {
  return (
    <div style={{
      background: '#fff2f0', border: '1px solid #ffccc7', borderRadius: '8px',
      padding: '10px', marginBottom: '16px', color: '#ff4d4f', fontSize: '14px'
    }}>
      {message}
    </div>
  );
}

export function CourierLogin({
  username,
  password,
  loading,
  error,
  onUsernameChange,
  onPasswordChange,
  onSubmit
}: {
  username: string;
  password: string;
  loading: boolean;
  error: string;
  onUsernameChange: (v: string) => void;
  onPasswordChange: (v: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}) {
  const inputStil = {
    width: '100%', padding: '14px', border: '1px solid #ddd',
    borderRadius: '10px', fontSize: '16px', boxSizing: 'border-box' as const
  };

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
        <form onSubmit={onSubmit}>
          <div style={{ marginBottom: '16px' }}>
            <input
              value={username}
              onChange={e => onUsernameChange(e.target.value)}
              placeholder="Kullanıcı adı"
              style={inputStil}
            />
          </div>
          <div style={{ marginBottom: '24px' }}>
            <input
              type="password"
              value={password}
              onChange={e => onPasswordChange(e.target.value)}
              placeholder="Şifre"
              style={inputStil}
            />
          </div>
          {error && <ErrorBox message={error} />}
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

function SayacKarti({ deger, etiket, renk }: { deger: number; etiket: string; renk: string }) {
  return (
    <div style={{
      flex: 1, background: renk, borderRadius: '12px', padding: '16px',
      color: 'white', textAlign: 'center'
    }}>
      <div style={{ fontSize: '28px', fontWeight: 700 }}>{deger}</div>
      <div style={{ fontSize: '13px', opacity: 0.9 }}>{etiket}</div>
    </div>
  );
}

function KargoKarti({ shipment, onSelect }: { shipment: any; onSelect: (s: any) => void }) {
  const dagitimda = shipment.currentStatus === 'Dağıtımda';

  return (
    <div
      onClick={() => onSelect(shipment)}
      style={{
        background: 'white', borderRadius: '12px', padding: '16px',
        cursor: 'pointer', boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
        borderLeft: `4px solid ${dagitimda ? '#fa8c16' : '#1890ff'}`
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
        <span style={{ fontFamily: 'monospace', fontWeight: 700, color: '#1890ff', fontSize: '15px' }}>
          {shipment.trackingCode}
        </span>
        <span style={{
          padding: '3px 10px', borderRadius: '10px', fontSize: '12px',
          background: dagitimda ? '#fff7e6' : '#e6f7ff',
          color: dagitimda ? '#fa8c16' : '#1890ff'
        }}>
          {shipment.currentStatus}
        </span>
      </div>
      <div style={{ fontWeight: 600, marginBottom: '4px' }}>{shipment.receiverName}</div>
      <div style={{ color: '#666', fontSize: '14px' }}>{shipment.receiverAddress}</div>
      <div style={{ color: '#999', fontSize: '13px', marginTop: '4px' }}>
        📍 {shipment.receiverCity}
      </div>
    </div>
  );
}

export function CourierList({
  shipments,
  fullName,
  onSelect,
  onLogout
}: {
  shipments: any[];
  fullName?: string;
  onSelect: (s: any) => void;
  onLogout: () => void;
}) {
  const sayi = (durum: string) =>
    shipments.filter(s => s.currentStatus === durum).length;

  return (
    <div style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <div style={{
        background: '#1a1a2e', padding: '20px',
        display: 'flex', alignItems: 'center', justifyContent: 'space-between'
      }}>
        <div>
          <div style={{ color: 'white', fontWeight: 700, fontSize: '18px' }}>🚚 Teslimatlarım</div>
          <div style={{ color: 'rgba(255,255,255,0.6)', fontSize: '13px' }}>{fullName}</div>
        </div>
        <button
          onClick={onLogout}
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
          <SayacKarti deger={sayi('Dağıtımda')} etiket="Dağıtımda" renk="#fa8c16" />
          <SayacKarti deger={sayi('Yolda')} etiket="Yolda" renk="#1890ff" />
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
              <KargoKarti key={s.id} shipment={s} onSelect={onSelect} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function KargoBilgileri({ shipment }: { shipment: any }) {
  const satirlar = [
    { label: '👤 Alıcı', value: shipment?.receiverName },
    { label: '📍 Adres', value: shipment?.receiverAddress },
    { label: '🏙️ Şehir', value: shipment?.receiverCity },
    { label: '⚖️ Ağırlık', value: `${shipment?.weight} kg` },
    { label: '📦 Durum', value: shipment?.currentStatus }
  ];

  return (
    <div style={{
      background: 'white', borderRadius: '12px', padding: '20px',
      marginBottom: '16px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)'
    }}>
      <div style={{
        fontFamily: 'monospace', color: '#1890ff', fontWeight: 700,
        fontSize: '16px', marginBottom: '16px'
      }}>
        {shipment?.trackingCode}
      </div>

      {satirlar.map(item => (
        <div key={item.label} style={{
          display: 'flex', justifyContent: 'space-between',
          padding: '10px 0', borderBottom: '1px solid #f0f0f0'
        }}>
          <span style={{ color: '#666', fontSize: '14px' }}>{item.label}</span>
          <span style={{ fontWeight: 500, fontSize: '14px', maxWidth: '200px', textAlign: 'right' }}>
            {item.value}
          </span>
        </div>
      ))}
    </div>
  );
}

function TeslimatDogrulama({
  deliveryCode,
  loading,
  error,
  success,
  onCodeChange,
  onDeliver
}: {
  deliveryCode: string;
  loading: boolean;
  error: string;
  success: string;
  onCodeChange: (v: string) => void;
  onDeliver: () => void;
}) {
  const devreDisi = loading || deliveryCode.length !== 6;

  return (
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
        onChange={e => onCodeChange(e.target.value.replace(/\D/g, '').slice(0, 6))}
        placeholder="_ _ _ _ _ _"
        maxLength={6}
        style={{
          width: '100%', padding: '16px', border: '2px solid #ddd',
          borderRadius: '10px', fontSize: '28px', fontWeight: 700,
          textAlign: 'center', letterSpacing: '8px',
          boxSizing: 'border-box', marginBottom: '16px'
        }}
      />

      {error && <ErrorBox message={error} />}

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
        onClick={onDeliver}
        disabled={devreDisi}
        style={{
          width: '100%', padding: '16px',
          background: devreDisi ? '#ccc' : '#52c41a',
          color: 'white', border: 'none', borderRadius: '10px',
          fontSize: '16px', fontWeight: 700,
          cursor: devreDisi ? 'not-allowed' : 'pointer'
        }}
      >
        {loading ? '⏳ İşleniyor...' : '✅ Teslim Et'}
      </button>
    </div>
  );
}

function YoldaBilgisi() {
  return (
    <div style={{
      background: 'white', borderRadius: '12px', padding: '24px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.06)', textAlign: 'center'
    }}>
      <div style={{ fontSize: '48px', marginBottom: '12px' }}>🚚</div>
      <div style={{ fontWeight: 600, color: '#1890ff', marginBottom: '8px' }}>Kargo Yolda</div>
      <div style={{ color: '#666', fontSize: '14px' }}>
        Teslimat kodu kargo dağıtıma çıktığında müşteriye gönderilecek.
      </div>
    </div>
  );
}

export function CourierDetail({
  shipment,
  deliveryCode,
  loading,
  error,
  success,
  onCodeChange,
  onDeliver,
  onBack
}: {
  shipment: any;
  deliveryCode: string;
  loading: boolean;
  error: string;
  success: string;
  onCodeChange: (v: string) => void;
  onDeliver: () => void;
  onBack: () => void;
}) {
  return (
    <div style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <div style={{
        background: '#1a1a2e', padding: '20px',
        display: 'flex', alignItems: 'center', gap: '16px'
      }}>
        <button
          onClick={onBack}
          style={{
            background: 'none', border: 'none', color: 'white',
            fontSize: '24px', cursor: 'pointer'
          }}
        >
          ←
        </button>
        <div style={{ color: 'white', fontWeight: 700, fontSize: '18px' }}>Teslimat Detayı</div>
      </div>

      <div style={{ padding: '16px' }}>
        <KargoBilgileri shipment={shipment} />

        {shipment?.currentStatus === 'Dağıtımda' ? (
          <TeslimatDogrulama
            deliveryCode={deliveryCode}
            loading={loading}
            error={error}
            success={success}
            onCodeChange={onCodeChange}
            onDeliver={onDeliver}
          />
        ) : (
          <YoldaBilgisi />
        )}
      </div>
    </div>
  );
}
