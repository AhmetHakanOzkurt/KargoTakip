import Modal from '../Modal';
import { transferDurumRengi, mesajRengi } from './statusStyles';

function DurumRozeti({ durum }: { durum: string }) {
  const renk = transferDurumRengi(durum);

  return (
    <span style={{
      padding: '3px 10px', borderRadius: '12px', fontSize: '12px', fontWeight: 500,
      background: renk.bg, color: renk.fg
    }}>{durum}</span>
  );
}

function KapatButonu({ onClick }: { onClick: () => void }) {
  return (
    <button onClick={onClick}
      style={{ width: '100%', padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer', marginTop: '8px' }}>
      Kapat
    </button>
  );
}

const tarihYaz = (t: string) => new Date(t).toLocaleString('tr-TR');

export function OutgoingTransfersModal({
  transfers,
  onClose
}: {
  transfers: any[];
  onClose: () => void;
}) {
  return (
    <Modal width="560px" scrollable>
      <h3 style={{ marginBottom: '20px' }}>📤 Giden Transfer Talepleri</h3>
      {transfers.length === 0 ? (
        <div style={{ color: '#999', textAlign: 'center', padding: '20px' }}>Giden transfer talebi yok.</div>
      ) : (
        transfers.map((t: any) => (
          <div key={t.id} style={{ border: '1px solid #eee', borderRadius: '8px', padding: '16px', marginBottom: '12px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
              <span style={{ fontWeight: 600 }}>#{t.id} — {t.hedefSube}</span>
              <DurumRozeti durum={t.status} />
            </div>
            <div style={{ fontSize: '13px', color: '#666' }}>
              {t.kargoSayisi} kargo • {tarihYaz(t.requestedAt)}
            </div>
            {t.scheduledAt && (
              <div style={{ fontSize: '13px', color: '#1890ff', marginTop: '4px' }}>
                Planlanan: {tarihYaz(t.scheduledAt)}
              </div>
            )}
          </div>
        ))
      )}
      <KapatButonu onClick={onClose} />
    </Modal>
  );
}

function GelenTalepKarti({
  transfer,
  onSelect
}: {
  transfer: any;
  onSelect: (t: any) => void;
}) {
  return (
    <div style={{ border: '1px solid #eee', borderRadius: '8px', padding: '16px', marginBottom: '12px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
        <span style={{ fontWeight: 600 }}>#{transfer.id} — {transfer.talepEdenSube}</span>
        <DurumRozeti durum={transfer.status} />
      </div>
      <div style={{ fontSize: '13px', color: '#666', marginBottom: '10px' }}>
        {transfer.kargoSayisi} kargo • {tarihYaz(transfer.requestedAt)}
      </div>
      {transfer.status === 'Bekliyor' && (
        <button
          onClick={() => onSelect(transfer)}
          style={{ padding: '6px 14px', background: '#1890ff', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontSize: '13px' }}
        >
          İncele / Yanıtla
        </button>
      )}
      {transfer.scheduledAt && (
        <div style={{ fontSize: '13px', color: '#52c41a', marginTop: '6px' }}>
          Planlanan: {tarihYaz(transfer.scheduledAt)}
        </div>
      )}
    </div>
  );
}

export function IncomingTransfersModal({
  transfers,
  message,
  onSelect,
  onClose
}: {
  transfers: any[];
  message: string;
  onSelect: (t: any) => void;
  onClose: () => void;
}) {
  const mesajStil = mesajRengi(message);

  return (
    <Modal width="560px" scrollable>
      <h3 style={{ marginBottom: '20px' }}>📥 Gelen Transfer Talepleri</h3>
      {message && (
        <div style={{ padding: '10px 14px', borderRadius: '8px', marginBottom: '16px', fontSize: '13px', background: mesajStil.bg, color: mesajStil.fg }}>
          {message}
        </div>
      )}
      {transfers.length === 0 ? (
        <div style={{ color: '#999', textAlign: 'center', padding: '20px' }}>Gelen transfer talebi yok.</div>
      ) : (
        transfers.map((t: any) => (
          <GelenTalepKarti key={t.id} transfer={t} onSelect={onSelect} />
        ))
      )}
      <KapatButonu onClick={onClose} />
    </Modal>
  );
}

export function RespondTransferModal({
  transfer,
  scheduleDate,
  rejectReason,
  onScheduleChange,
  onReasonChange,
  onBack,
  onReject,
  onApprove
}: {
  transfer: any;
  scheduleDate: string;
  rejectReason: string;
  onScheduleChange: (v: string) => void;
  onReasonChange: (v: string) => void;
  onBack: () => void;
  onReject: () => void;
  onApprove: () => void;
}) {
  return (
    <Modal width="440px" zIndex={1100}>
      <h3 style={{ marginBottom: '4px' }}>Transfer Talebi #{transfer.id}</h3>
      <p style={{ color: '#666', fontSize: '13px', marginBottom: '20px' }}>
        {transfer.talepEdenSube} şubesinden {transfer.kargoSayisi} kargo
      </p>

      <div style={{ marginBottom: '16px' }}>
        <label style={{ fontSize: '13px', color: '#555', fontWeight: 600, display: 'block', marginBottom: '6px' }}>
          Planlanan Transfer Tarihi (Onayla için)
        </label>
        <input
          type="datetime-local"
          value={scheduleDate}
          onChange={e => onScheduleChange(e.target.value)}
          style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', boxSizing: 'border-box' }}
        />
      </div>

      <div style={{ marginBottom: '20px' }}>
        <label style={{ fontSize: '13px', color: '#555', fontWeight: 600, display: 'block', marginBottom: '6px' }}>
          Red Sebebi (Reddet için)
        </label>
        <input
          value={rejectReason}
          onChange={e => onReasonChange(e.target.value)}
          placeholder="Sebep yazın..."
          style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '8px', fontSize: '14px', boxSizing: 'border-box' }}
        />
      </div>

      <div style={{ display: 'flex', gap: '10px' }}>
        <button onClick={onBack}
          style={{ flex: 1, padding: '10px', background: '#f0f0f0', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>
          Geri
        </button>
        <button onClick={onReject} disabled={!rejectReason}
          style={{ flex: 1, padding: '10px', background: !rejectReason ? '#ccc' : '#ff4d4f', color: 'white', border: 'none', borderRadius: '8px', cursor: !rejectReason ? 'not-allowed' : 'pointer' }}>
          Reddet
        </button>
        <button onClick={onApprove} disabled={!scheduleDate}
          style={{ flex: 1, padding: '10px', background: !scheduleDate ? '#ccc' : '#52c41a', color: 'white', border: 'none', borderRadius: '8px', cursor: !scheduleDate ? 'not-allowed' : 'pointer' }}>
          Onayla
        </button>
      </div>
    </Modal>
  );
}
