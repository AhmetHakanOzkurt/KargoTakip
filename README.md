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
| GET | /api/orders | Tüm kargoları listele |
| GET | /api/orders/{id} | Tek kargo getir |
| POST | /api/orders | Yeni kargo oluştur |
| PUT | /api/orders/{id}/status | Kargo durumunu güncelle |

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
| GET | /api/notifications/{branchId} | Şube bildirimlerini listele |
| GET | /api/notifications/{branchId}/unread-count | Okunmamış bildirim sayısı |
| PUT | /api/notifications/{id}/read | Bildirimi okundu yap |

### ReportService
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/reports/summary | Genel özet raporu |
| GET | /api/reports/branches | Şube bazlı rapor |
| GET | /api/reports/vehicles | Araç bazlı rapor |
| GET | /api/reports/daily | Günlük rapor |
| GET | /api/reports/shipments | Tarih aralığına göre kargo raporu |

## 🔒 Güvenlik

- JWT Bearer Token ile kimlik doğrulama
- BCrypt ile şifre hashleme
- Rate limiting: Login endpoint'i 5 dakikada 5 deneme
- FluentValidation ile input validasyonu
- Global exception handler

## 📁 Proje Yapısı
KargoTakip/
├── KargoTakip.Infrastructure/   # Veritabanı modelleri ve DbContext
│   ├── Models/                  # Entity sınıfları
│   ├── Data/                    # DbContext ve Factory
│   └── Migrations/              # EF Migration dosyaları
├── AuthService/                 # Kimlik doğrulama servisi
├── OrderService/                # Kargo yönetim servisi
├── VehicleService/              # Araç yönetim servisi
├── NotificationService/         # Bildirim servisi
└── ReportService/               # Raporlama servisi

## 👨‍💻 Geliştirici

**Ahmet** — Bilgisayar Mühendisliği Öğrencisi