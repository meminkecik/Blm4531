# Nearest - Oto Kurtarma Firmaları API

Bu proje, kullanıcıların konumlarına göre en yakın oto kurtarma firmalarını bulabilecekleri bir .NET 8 Web API uygulamasıdır.

## Özellikler

- 🔐 **Firma Kayıt ve Giriş**: Oto kurtarma firmaları kayıt olabilir ve JWT token ile giriş yapabilir
- 📍 **Konum Tabanlı Arama**: Kullanıcılar konum bilgilerini paylaşarak en yakın firmaları bulabilir
- 🎫 **Ticket Sistemi**: İletişim için ticket sistemi
- 🐳 **Docker Desteği**: Tamamen containerize edilmiş
- 🗄️ **PostgreSQL**: Güçlü veritabanı desteği

## Teknolojiler

- .NET 8
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- AutoMapper
- Docker & Docker Compose
- Swagger/OpenAPI

## Kurulum

### Docker ile Çalıştırma (Önerilen)

1. Projeyi klonlayın:
```bash
git clone <repository-url>
cd Nearest
```

2. Docker Compose ile çalıştırın:
```bash
docker-compose up --build
```

Bu komut hem PostgreSQL veritabanını hem de API'yi başlatacaktır.

### Manuel Kurulum

1. PostgreSQL veritabanını kurun ve çalıştırın
2. `appsettings.json` dosyasındaki connection string'i güncelleyin
3. Migration'ları çalıştırın:
```bash
dotnet ef database update
```

4. Uygulamayı çalıştırın:
```bash
dotnet run
```

## API Endpoints

### Kimlik Doğrulama
- `POST /api/auth/register` - Firma kayıt
- `POST /api/auth/login` - Firma giriş
- `POST /api/admin/login` - Admin giriş

### Profil
- `GET /api/profile` - Profil bilgileri (Admin veya Company, JWT gerekli)

### Firmalar
- `GET /api/companies` - Tüm firmaları listele
- `GET /api/companies/nearest?latitude={lat}&longitude={lon}&provinceId={provinceId}&districtId={districtId}&limit={limit}` - İl/ilçe filtreli en yakın firmaları bul

### Tickets
- `POST /api/tickets` - Ticket oluştur (kullanıcılar için)
- `GET /api/tickets` - Firmaların ticket'larını listele (JWT gerekli)
- `PUT /api/tickets/{id}/status` - Ticket durumunu güncelle (JWT gerekli)

### Tow Trucks
- `POST /api/towtrucks` - Yeni çekici kaydı oluştur (Company rolü)
- `GET /api/towtrucks/my?includeInactive={bool}` - Firmanın çekicilerini listele (pasifleri görmek için includeInactive=true)
- `PUT /api/towtrucks/{id}` - Şoför, bölge veya aktiflik bilgisini güncelle
- `PUT /api/towtrucks/{id}/deactivate` - Çekiciyi tek adımda pasif duruma getir
- `DELETE /api/towtrucks/{id}` - Çekiciyi ve bölgelerini tamamen sil

## Swagger UI

Uygulama çalıştıktan sonra Swagger UI'ya şu adresten erişebilirsiniz:
- http://localhost:5000/swagger (HTTP)
- https://localhost:5001/swagger (HTTPS)

## Veritabanı Modelleri

### Company
- Firma bilgileri (isim, telefon, adres, konum)
- Hizmet verdiği bölgeler
- JWT authentication için gerekli bilgiler

### Ticket
- Kullanıcı iletişim formu
- Firma ile ilişkili
- Durum takibi

### UserLocation
- Kullanıcı konum bilgileri
- Session takibi

## Güvenlik

- JWT token tabanlı authentication
- Password hashing (SHA256)
- CORS yapılandırması
- Input validation

## Geliştirme

Proje geliştirme için:

1. Visual Studio 2022 veya VS Code kullanın
2. .NET 8 SDK'yı yükleyin
3. PostgreSQL'i local olarak çalıştırın
4. `dotnet ef tools` global olarak yükleyin

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır.
