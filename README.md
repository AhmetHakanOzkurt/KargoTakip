# 🚚 KargoTakip — Mikroservis Tabanlı Kargo Yönetim Sistemi

## 📋 Proje Hakkında

KargoTakip, bir kargo firmasının tüm operasyonlarını yönetmek için geliştirilmiş mikroservis tabanlı bir yazılım sistemidir. Sistem; kargo oluşturma, araç atama, durum takibi, bildirim ve raporlama gibi temel kargo süreçlerini kapsamaktadır.

## 🏗️ Mimari

Sistem aşağıdaki mikroservislerden oluşmaktadır:

| Servis | Port | Açıklama |
|--------|------|----------|
| AuthService | 7205 | Kullanıcı girişi ve JWT token yönetimi |
| OrderService | 7029 | Kargo oluşturma, listeleme ve durum güncelleme |
| VehicleService | 7139 | Araç yönetimi ve otomatik araç atama |
| NotificationService | 5154 | RabbitMQ event'lerini dinleyerek bildirim oluşturma |
| ReportService | 5048 | Kargo, araç ve şube bazlı raporlama |

## 🧩 Kullanılan Teknolojiler

- **Backend:** C# / .NET 8 Web API
- **Veritabanı:** MS SQL Server (Entity Framework Core)
- **Mesaj Kuyruğu:** RabbitMQ
- **Container:** Docker
- **Kimlik Doğrulama:** JWT Bearer Token
- **Şifreleme:** BCrypt
- **Loglama:** Serilog
- **Validasyon:** FluentValidation
- **Rate Limiting:** AspNetCoreRateLimit

## 🗄️ Veritabanı Şeması

Sistem 10 tablodan oluşmaktadır:

- `Cities` — Şehirler
- `Branches` — Şubeler
- `VehicleTypes` — Araç tipleri
- `Users` — Personel
- `Vehicles` — Araçlar
- `Shipments` — Kargolar
- `ShipmentStatusHistory` — Durum geçmişi
- `TransferRequests` — Şubeler arası transfer talepleri
- `TransferRequestItems` — Transfer talebindeki kargolar
- `Notifications` — Bildirimler

## 🚀 Kurulum

### Gereksinimler

- .NET 8 SDK
- SQL Server / SQL Server Express
- Docker Desktop
- Visual Studio 2022

### 1. Repoyu Klonla

```bash
git clone https://github.com/KULLANICI_ADIN/KargoTakip.git
cd KargoTakip
```

### 2. RabbitMQ'yu Başlat

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

### 3. Veritabanını Oluştur

```bash
cd KargoTakip.Infrastructure
dotnet ef database update
```

### 4. Servisleri Başlat

Her servis için ayrı terminal aç:

```bash
cd AuthService && dotnet run
cd OrderService && dotnet run
cd VehicleService && dotnet run
cd NotificationService && dotnet run
cd ReportService && dotnet run
```

### 5. Test Verisi Ekle

SSMS'de şunu çalıştır:

```sql
USE KargoTakipDB;

INSERT INTO Cities (Name, Region) VALUES ('İstanbul', 'Marmara');
INSERT INTO Branches (Name, CityId, Address, IsActive, CreatedAt)
VALUES ('İstanbul A Şubesi', 1, 'Kadıköy, İstanbul', 1, GETDATE());

INSERT INTO VehicleTypes (Name, MaxCapacity, RouteType)
VALUES ('Motokurye', 5, 'CityOnly'),
       ('Minivan', 20, 'Both'),
       ('Kamyon', 100, 'Intercity');
```

## 🔄 Sistem Akışı

Kullanıcı AuthService'e login olur → JWT token alır
Token ile OrderService'e kargo oluşturur
OrderService → VehicleService'e araç atama isteği gönderir
OrderService → RabbitMQ'ya "kargo_olusturuldu" eventi yayınlar
NotificationService eventi dinler → bildirim oluşturur
Durum güncellenince → yeni event → yeni bildirim
ReportService raporları sunar


## 📡 API Endpoints

### AuthService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/auth/login | Giriş yap, token al |

### OrderService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/orders?sayfa=1&sayfaBoyutu=50 | Kargoları listele (sayfalı) |
| GET | /api/orders/{id} | Tek kargo getir |
| GET | /api/orders/cities | Şehir listesi |
| GET | /api/orders/track/{trackingCode} | Müşteri takibi (token gerekmez) |
| POST | /api/orders | Yeni kargo oluştur |
| PUT | /api/orders/{id}/status | Kargo durumunu güncelle |
| PUT | /api/orders/{id}/deliver | Teslimat kodu ile teslim et |

Şube dışı erişim engellidir: Admin olmayan kullanıcılar yalnızca kendi
şubelerinin kargolarını görebilir ve değiştirebilir. `branchId` ve
`userId` istemciden değil JWT claim'lerinden okunur.

### TransferService (OrderService içinde)
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/transfers/outgoing | Gönderilen transfer talepleri |
| GET | /api/transfers/incoming | Gelen transfer talepleri |
| POST | /api/transfers | Transfer talebi oluştur |
| PUT | /api/transfers/{id}/approve | Talebi onayla |
| PUT | /api/transfers/{id}/reject | Talebi reddet |

### ConsolidationService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/consolidation/plans | Konsolidasyon planlarını listele |
| POST | /api/consolidation/run | Konsolidasyon algoritmasını çalıştır |

Araçlar doluluk eşiğine ulaşınca veya kargolar `MaxWaitHours` süresini
aşınca sefer planı oluşturulur; eşik altındaysa en yakın 3 komşu şehre
giden kargolar da aynı sefere eklenir.

### VehicleService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/vehicles | Tüm araçları listele |
| GET | /api/vehicles/available | Müsait araçları listele |
| GET | /api/vehicles/{id} | Tek araç getir |
| POST | /api/vehicles | Yeni araç ekle |

### NotificationService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/notifications/{branchId}?sayfa=1&sayfaBoyutu=50 | Şube bildirimleri (sayfalı) |
| GET | /api/notifications/{branchId}/unread-count | Okunmamış bildirim sayısı |
| PUT | /api/notifications/{id}/read | Bildirimi okundu yap |

### ReportService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/reports/summary | Genel özet raporu |
| GET | /api/reports/branches | Şube bazlı rapor |
| GET | /api/reports/vehicles | Araç bazlı rapor |
| GET | /api/reports/daily | Günlük rapor |
| GET | /api/reports/shipments | Tarih aralığına göre kargo raporu (sayfalı) |

Listeleme uçları `{ toplamKayit, sayfa, sayfaBoyutu, kayitlar }` şeklinde
zarf döner.

## 🔒 Güvenlik

- JWT Bearer Token ile kimlik doğrulama
- BCrypt ile şifre hashleme
- Rate limiting: Login endpoint'i 5 dakikada 5 deneme
- FluentValidation ile input validasyonu
- Global exception handler
- Şube bazlı yetkilendirme: kullanıcı kimliği ve şubesi token claim'lerinden
  alınır, istemciden gelen değerlere güvenilmez
- Teslimat ve takip kodları `RandomNumberGenerator` ile üretilir
- Swagger yalnızca Development ortamında açıktır

### Secret yönetimi

Hiçbir secret repoda tutulmaz. `JwtSettings:SecretKey` boştur ve ortam
değişkeninden gelmelidir; tanımsız veya 32 karakterden kısaysa servis
açılışta durur.

```bash
cp .env.example .env
# .env icindeki JWT_SECRET, DB_PASSWORD ve EMAIL_PASSWORD doldurulmali
```

`JWT_SECRET` üretmek için: `openssl rand -base64 48`

## 📁 Proje Yapısı
KargoTakip/
├── KargoTakip.Infrastructure/   # Veritabanı modelleri ve DbContext
│   ├── Models/                  # Entity sınıfları
│   ├── Data/                    # DbContext ve Factory
│   └── Migrations/              # EF Migration dosyaları
│   └── Messaging/               # Servisler arasi paylasilan event sozlesmeleri
├── AuthService/                 # Kimlik doğrulama servisi
├── OrderService/                # Kargo ve transfer yönetimi
├── VehicleService/              # Araç yönetim servisi
├── NotificationService/         # Bildirim ve e-posta servisi
├── ReportService/               # Raporlama servisi
├── ConsolidationService/        # Sefer konsolidasyon motoru
├── KargoTakip.Tests/            # Birim testleri (xunit)
├── dashboard/                   # React yönetim arayüzü
└── Directory.Packages.props     # Merkezi paket sürüm yönetimi

## 🧪 Test

```bash
dotnet test KargoTakip.sln
```

Her push ve pull request'te GitHub Actions .NET build+test, dashboard
build ve `docker compose config` doğrulamasını çalıştırır.

## 🗃️ Migration

Şema değişikliğinden sonra:

```bash
dotnet ef migrations add <Ad> --project KargoTakip.Infrastructure --startup-project AuthService
```

Production'da migration'ları yalnızca `auth-service` uygular
(`Database__RunMigrations=true`); diğer servisler onun sağlıklı olmasını
bekler.

## 👨‍💻 Geliştirici

**Ahmet** — Bilgisayar Mühendisliği Öğrencisi