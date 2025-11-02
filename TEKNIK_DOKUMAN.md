# NEAREST - OTO KURTARMA FIRMALARI API
## Teknik Dokümantasyon

Bu dokümantasyon, Nearest API projesinin baştan sona tüm bileşenlerinin ne işe yaradığını, nasıl çalıştığını ve nasıl kullanıldığını detaylı bir şekilde açıklar.

---

## 📋 İÇİNDEKİLER

1. [Genel Bakış](#genel-bakış)
2. [Proje Mimarisi](#proje-mimarisi)
3. [Kurulum ve Konfigürasyon](#kurulum-ve-konfigürasyon)
4. [Veritabanı Yapısı](#veritabanı-yapısı)
5. [Servisler (Services)](#servisler-services)
6. [Controller'lar](#controllerlar)
7. [Repository Pattern](#repository-pattern)
8. [Güvenlik](#güvenlik)
9. [API Endpoints](#api-endpoints)
10. [Docker ve Deployment](#docker-ve-deployment)

---

## GENEL BAKIŞ

### Proje Amacı
Nearest, kullanıcıların GPS koordinatlarını kullanarak en yakın oto kurtarma firmalarını bulmasını sağlayan bir Web API uygulamasıdır.

### Kullanılan Teknolojiler
- **.NET 8**: Modern C# geliştirme framework'ü
- **PostgreSQL**: İlişkisel veritabanı yönetim sistemi
- **Entity Framework Core**: ORM (Object-Relational Mapping) aracı
- **JWT (JSON Web Token)**: Kimlik doğrulama ve yetkilendirme
- **AutoMapper**: Nesne dönüşüm kütüphanesi
- **Swagger/OpenAPI**: API dokümantasyonu
- **Docker**: Containerization

---

## PROJE MİMARİSİ

Proje katmanlı mimari (Layered Architecture) kullanır:

```
┌─────────────────────────────────────────┐
│         CONTROLLER LAYER                │  ← HTTP isteklerini alır
│  (API Endpoint'leri)                    │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│          SERVICE LAYER                  │  ← İş mantığı
│  (Business Logic)                       │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│        REPOSITORY LAYER                 │  ← Veri erişimi
│  (Data Access)                          │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         DATA LAYER                      │  ← Veritabanı
│  (Entity Framework + PostgreSQL)        │
└─────────────────────────────────────────┘
```

---

## KURULUM VE KONFİGÜRASYON

### Program.cs - Uygulama Başlangıcı

`Program.cs` dosyası uygulamanın kalbidir. Burada tüm servisler kaydedilir ve HTTP pipeline yapılandırılır.

#### Yapılandırma Bölümleri:

**1. Entity Framework DbContext Kaydı (Satır 16-17)**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```
**Ne işe yarar?**
- Veritabanı bağlantısını yapılandırır
- PostgreSQL provider'ını kullanır
- Connection string'i appsettings.json'dan okur

**2. AutoMapper Kaydı (Satır 20)**
```csharp
builder.Services.AddAutoMapper(typeof(CompanyMappingProfile), ...);
```
**Ne işe yarar?**
- Entity ↔ DTO dönüşümlerini otomatikleştirir
- Mapping profillerini tarar ve kaydeder

**3. JWT Authentication (Satır 23-34)**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```
**Ne işe yarar?**
- Token doğrulama parametrelerini ayarlar
- HMAC SHA256 imzalama algoritmasını kullanır
- 7 günlük token geçerliliği

**4. Servis Kayıtları (Satır 39-54)**
```csharp
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ILocationService, LocationService>();
```
**Ne işe yarar?**
- Dependency Injection container'a servisleri ekler
- Scoped: Her HTTP request'te yeni instance oluşturur

**5. CORS Politikası (Satır 60-68)**
```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
```
**Ne işe yarar?**
- Cross-Origin Resource Sharing ayarlar
- Tüm origin'lerden isteklere izin verir (Development için)

**6. Swagger Yapılandırması (Satır 72-101)**
```csharp
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
});
```
**Ne işe yarar?**
- API dokümantasyonu oluşturur
- JWT token desteği ekler
- Swagger UI'da "Authorize" butonu görünür

**7. Middleware Pipeline (Satır 106-118)**
```csharp
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
```
**Ne işe yarar?**
- `UseSwagger`: Swagger JSON endpoint'i
- `UseHttpsRedirection`: HTTP → HTTPS yönlendirme
- `UseCors`: CORS middleware'i
- `UseAuthentication`: Token doğrulama
- `UseAuthorization`: Yetkilendirme kontrolü
- `UseStaticFiles`: wwwroot klasöründeki dosyaları serve eder

**8. Otomatik Migration ve Admin Oluşturma (Satır 126-134)**
```csharp
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    
    var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
    await adminService.CreateDefaultAdminAsync();
}
```
**Ne işe yarar?**
- Uygulama başlarken migration'ları çalıştırır
- Default admin kullanıcısı oluşturur (yoksa)
- Email: nearestmek@gmail.com, Şifre: 145236Aa**

---

## VERİTABANI YAPISI

### ApplicationDbContext

DbContext, Entity Framework Core'un ana bileşenidir ve veritabanı ile C# nesneleri arasında köprü görevi yapar.

#### DbSet'ler (Satır 13-23)

**Company**: Firma bilgileri
**Ticket**: İletişim talepleri
**UserLocation**: Kullanıcı konum bilgileri
**Admin**: Yönetici kullanıcıları
**TowTruck**: Çekici araçlar
**TowTruckArea**: Çekici çalışma bölgeleri
**City**: İller
**District**: İlçeler
**CityDistrict**: İl-İlçe ilişkileri (Many-to-Many)

#### OnModelCreating Yapılandırmaları

**Company Unique Indexes (Satır 30-34)**
```csharp
entity.HasIndex(e => e.Email).IsUnique();
entity.HasIndex(e => e.PhoneNumber).IsUnique();
```
**Ne işe yarar?**
- Email ve telefon numarası benzersiz olmalı
- Veritabanı seviyesinde constraint

**Ticket Foreign Key (Satır 43-49)**
```csharp
entity.HasOne(t => t.Company)
      .WithMany(c => c.Tickets)
      .HasForeignKey(t => t.CompanyId)
      .OnDelete(DeleteBehavior.SetNull);
```
**Ne işe yarar?**
- Ticket → Company ilişkisi
- Firma silinirse ticket'lar silinmez (SetNull)

**TowTruck Unique License Plate (Satır 58-59)**
```csharp
entity.HasIndex(t => t.LicensePlate).IsUnique();
```
**Ne işe yarar?**
- Plaka numarası sistem genelinde benzersiz
- Aynı plaka birden fazla kez kaydedilemez

---

## SERVİSLER (SERVICES)

Servisler iş mantığını içerir ve Controller'lardan çağrılır.

### 1. JwtService

JWT token üretimi ve doğrulama işlemlerini yapar.

#### GenerateToken(Company) - Satır 18-39
```csharp
var tokenDescriptor = new SecurityTokenDescriptor {
    Subject = new ClaimsIdentity(new[] {
        new Claim("CompanyId", company.Id.ToString()),
        new Claim("Role", "Company")
    }),
    Expires = DateTime.UtcNow.AddDays(7)
};
```
**Ne işe yarar?**
- Firma için JWT token üretir
- Token içinde firma ID ve rol bilgisi var
- 7 gün geçerli
- HMAC SHA256 ile imzalanır

#### GenerateToken(Admin) - Satır 41-62
**Ne işe yarar?**
- Admin için token üretir
- AdminId ve "Admin" rolü içerir

#### ValidateToken - Satır 64-86
**Ne işe yarar?**
- Token'ın geçerli olup olmadığını kontrol eder
- İmza doğrulaması yapar
- Süresi dolmuş token'ları reddeder

---

### 2. LocationService

Konum tabanlı arama işlemlerini yönetir.

#### GetNearestCompaniesAsync - Satır 19-43
```csharp
var companies = await _context.Companies
    .Where(c => c.IsActive && c.Latitude.HasValue && c.Longitude.HasValue)
    .ToListAsync();

foreach (var company in companyDtos) {
    company.Distance = CalculateDistance(latitude, longitude, 
        company.Latitude.Value, company.Longitude.Value);
}
```
**Ne işe yarar?**
- Aktif firmaları çeker
- Her firma için mesafeyi hesaplar
- Mesafeye göre sıralar ve limit kadar döndürür

#### CalculateDistance - Satır 54-68
**Haversine Formülü Kullanımı:**
```
a = sin²(Δlat/2) + cos(lat1) × cos(lat2) × sin²(Δlon/2)
c = 2 × atan2(√a, √(1-a))
Distance = R × c
```
**Ne işe yarar?**
- İki GPS noktası arasındaki mesafeyi km cinsinden hesaplar
- Dünya yarıçapı: 6371 km
- Great Circle Distance algoritması kullanır

---

### 3. AdminService

Yönetici işlemlerini yönetir.

#### LoginAsync - Satır 21-39
```csharp
var admin = await _adminRepository.GetByEmailAsync(loginDto.Email);
if (admin == null || !VerifyPassword(...)) {
    return null; // Başarısız giriş
}
var token = GenerateAdminToken(admin);
return new AdminAuthResponseDto { Token = token, ... };
```
**Ne işe yarar?**
- Email ve şifre doğrulama
- SHA256 hash karşılaştırması
- JWT token üretimi
- Admin bilgilerini döndürme

#### CreateDefaultAdminAsync - Satır 47-67
**Ne işe yarar?**
- Sistem başlarken default admin oluşturur
- Email: nearestmek@gmail.com
- Şifre: 145236Aa**
- Aynı admin zaten varsa oluşturmaz

---

### 4. TicketService

İletişim taleplerini yönetir.

#### CreateTicketAsync - Satır 30-41
```csharp
var ticket = _mapper.Map<Ticket>(ticketDto);
_context.Tickets.Add(ticket);
await _context.SaveChangesAsync();

await _emailService.SendTicketNotificationAsync(ticketDto);
```
**Ne işe yarar?**
- Ticket oluşturur
- Admin'e email bildirimi gönderir
- AutoMapper ile DTO → Entity dönüşümü

---

### 5. TowTruckService

Çekici yönetimi işlemleri.

#### CreateTowTruckAsync - Satır 26-85
```csharp
// Firma kontrolü
var companyExists = await _context.Companies.AnyAsync(...);

// Plaka benzersizlik kontrolü
var normalizedPlate = dto.LicensePlate.Trim().ToUpperInvariant();
var existsPlate = await _context.TowTrucks.AnyAsync(...);

// Fotoğraf yükleme
var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(...)}";
await driverPhoto.CopyToAsync(stream);

// Çalışma bölgelerini parse et
var areas = JsonSerializer.Deserialize<List<TowTruckAreaInputDto>>(dto.AreasJson);
foreach (var area in areas) {
    var cityName = await _addressService.GetCityNameAsync(area.ProvinceId);
    towTruck.OperatingAreas.Add(new TowTruckArea { ... });
}
```
**Ne işe yarar?**
- Çekici kaydı oluşturur
- Plaka benzersizliğini kontrol eder
- Şoför fotoğrafını yükler
- Çalışma bölgelerini parse eder ve ID'lerden isimlere çevirir

---

### 6. AddressService

Türkiye adres verilerini yönetir.

#### GetCitiesAsync - Satır 30-40
**Ne işe yarar?**
- Tüm illeri çeker
- CityResponseDto formatında döndürür
- Status: "SUCCESS" veya "ERROR"

#### GetDistrictsByCityIdAsync - Satır 42-62
**Ne işe yarar?**
- Belirtilen ile ait ilçeleri çeker
- İl bulunamazsa boş liste döner

#### UpdateAddressAsync - Satır 64-72
**Ne işe yarar?**
- External API'den (turkiyeapi.dev) adres verilerini çeker
- AddressHelperService'i çağırır

---

### 7. AddressHelperService

External API ile adres verisi senkronizasyonu.

#### FetchRemoteAddressAsync - Satır 27-62
```csharp
var response = await _httpClient.GetStringAsync("https://turkiyeapi.dev/api/v1/provinces");
var provincesResponse = JsonSerializer.Deserialize<ProvincesResponseDto>(response);

await UpdateOrSaveCitiesAsync(provinces);
await UpdateOrSaveDistrictsAsync(provinces);
await UpdateOrSaveCityDistrictsAsync(provinces);
```
**Ne işe yarar?**
- Türkiye API'den 81 ili ve tüm ilçeleri çeker
- Veritabanını senkronize eder
- Yeni kayıtları ekler, mevcutları günceller

---

### 8. EmailService

Email gönderim işlemleri.

#### SendEmailAsync - Satır 19-48
```csharp
using var client = new SmtpClient(smtpHost, smtpPort);
client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
client.EnableSsl = true;

var message = new MailMessage();
message.From = new MailAddress(smtpUsername, "Nearest Oto Kurtarma");
message.To.Add(emailDto.To);
message.Subject = emailDto.Subject;
message.Body = emailDto.Body;
message.IsBodyHtml = emailDto.IsHtml;

await client.SendMailAsync(message);
```
**Ne işe yarar?**
- SMTP üzerinden email gönderir
- Gmail SMTP kullanır
- HTML veya plain text destekler
- Log kaydı tutar

#### SendTicketNotificationAsync - Satır 50-77
**Ne işe yarar?**
- Yeni ticket geldiğinde admin'e bildirim gönderir
- HTML formatında detaylı bilgi içerir
- Gönderen, konu, mesaj bilgilerini içerir

---

## CONTROLLER'LAR

Controller'lar HTTP isteklerini alır ve servislere yönlendirir.

### 1. AuthController

Firma kayıt ve giriş işlemleri.

#### POST /api/auth/register
```csharp
[HttpPost("register")]
public async Task<ActionResult<AuthResponseDto>> Register(CompanyRegistrationDto dto)
{
    // Email ve telefon benzersizlik kontrolü
    if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
        return BadRequest("Bu email adresi zaten kullanılıyor.");
    
    // Şifre hash'leme
    company.PasswordHash = HashPassword(dto.Password);
    
    // JWT token üret
    var token = _jwtService.GenerateToken(company);
    
    return Ok(new AuthResponseDto { Token = token, ... });
}
```
**Ne işe yarar?**
- Yeni firma kaydı
- Email ve telefon unique kontrolü
- SHA256 şifre hash'i
- JWT token döndürme

#### POST /api/auth/login
**Ne işe yarar?**
- Firma girişi
- Email/şifre doğrulama
- 7 günlük JWT token

---

### 2. ProfileController

Merkezi profil yönetimi.

#### GET /api/profile
```csharp
[HttpGet]
[Authorize]
public async Task<IActionResult> GetProfile()
{
    var role = User.FindFirst("Role")?.Value;
    
    if (role == "Admin") {
        var admin = await _adminService.GetByIdAsync(adminId);
        return Ok(admin);
    }
    else if (role == "Company") {
        var company = await _context.Companies.FirstOrDefaultAsync(...);
        return Ok(_mapper.Map<CompanyDto>(company));
    }
}
```
**Ne işe yarar?**
- JWT token'dan rol bilgisi okur
- Admin ise → AdminDto
- Company ise → CompanyDto
- Sadece kendi profiline erişim

---

### 3. CompaniesController

Firma işlemleri.

#### GET /api/companies/nearest
**Parametreler:**
- `latitude`: Enlem
- `longitude`: Boylam
- `limit`: Döndürülecek firma sayısı (1-50)

**Ne işe yarar?**
- Haversine formülü ile mesafe hesaplama
- En yakın aktif firmaları döndürme
- Mesafe bilgisi ile birlikte

#### GET /api/companies
**Ne işe yarar?**
- Tüm aktif firmaları listeler
- Admin paneli için

#### PUT /api/companies/me
**Ne işe yarar?**
- Firma kendi bilgilerini günceller
- Partial update (sadece gönderilen alanlar)
- İl/ilçe ID'lerini isimlere çevirir

---

### 4. TicketsController

İletişim talebi yönetimi.

#### POST /api/tickets
**Ne işe yarar?**
- Herkes yeni ticket oluşturabilir
- Admin'e email bildirimi
- Ticket ID döndürür

#### GET /api/tickets
**Ne işe yarar?**
- Admin: Tüm ticket'lar
- Company: Sadece kendi ticket'ları
- Tarihe göre azalan sıralama

#### PUT /api/tickets/{id}/status
**Ne işe yarar?**
- Ticket durumunu günceller
- Durumlar: New, InProgress, Resolved, Closed
- Admin her ticket'ı değiştirebilir
- Company sadece kendi ticket'larını

---

### 5. TowTrucksController

Çekici yönetimi.

#### POST /api/towtrucks
**Ne işe yarar?**
- Yeni çekici kaydı
- Form-data desteği (fotoğraf)
- Plaka benzersizlik kontrolü
- Çalışma bölgeleri JSON formatında

#### GET /api/towtrucks/my
**Ne işe yarar?**
- Firma kendi çekicilerini listeler
- Çalışma bölgeleri ile birlikte

---

### 6. AdminController

Yönetici işlemleri.

#### POST /api/admin/login
**Ne işe yarar?**
- Admin girişi
- JWT token döndürür

#### PUT /api/admin/address
**Ne işe yarar?**
- Türkiye API'den adres verilerini günceller
- Uzun sürebilir (81 il × ~100 ilçe)
- Sadece Admin rolü

#### GET /api/admin/tickets
**Ne işe yarar?**
- Tüm ticket'ları görüntüler
- Admin paneli için

---

### 7. AddressController

Adres verisi endpoint'leri.

#### GET /api/address/cities
**Ne işe yarar?**
- 81 il listesi
- Herkes erişebilir

#### GET /api/address/districts/{provinceId}
**Ne işe yarar?**
- İl ID'sine göre ilçeler
- Örnek: /api/address/districts/34 → İstanbul ilçeleri

---

## REPOSITORY PATTERN

Repository katmanı veri erişimi işlemlerini soyutlar.

### AdminRepository
- `GetByEmailAsync`: Email'e göre admin bulur
- `GetByIdAsync`: ID'ye göre admin bulur
- `AddAsync`: Yeni admin ekler
- `IsDefaultAdminExistsAsync`: Default admin var mı?

### CityRepository
- `GetAllAsync`: Tüm iller
- `GetByProvinceIdAsync`: Province ID'ye göre il
- `AddAsync`, `UpdateAsync`: CRUD işlemleri

### DistrictRepository
- `GetAllAsync`: Tüm ilçeler
- `GetByDistrictIdAsync`: District ID'ye göre
- CRUD işlemleri

### CityDistrictRepository
- `GetByCityAsync`: İle ait tüm ilçeler
- `GetByCityAndDistrictAsync`: İl-ilçe ilişkisi var mı?
- Many-to-Many ilişkisi yönetimi

---

## GÜVENLİK

### 1. Şifre Hash'leme
```csharp
private string HashPassword(string password) {
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}
```
**Nasıl Çalışır?**
- SHA256 hash algoritması
- Base64 encoding
- Veritabanında düz metin saklanmaz

### 2. JWT Authentication

**Token Yapısı:**
```json
{
    "CompanyId": "123",
    "Role": "Company",
    "Email": "firma@example.com",
    "exp": 1234567890
}
```

**Doğrulama:**
- HMAC SHA256 imzalama
- Signature kontrolü
- Expiration kontrolü
- Clock skew: 0

### 3. CORS Politikası
- Development: AllowAll
- Production: Spesifik origin'ler

### 4. Authorization
- Role-based access control
- Claim-based yetkilendirme
- [Authorize] attribute

---

## API ENDPOINTS

### Public Endpoints (Token Gerektirmez)
- `POST /api/auth/register` - Firma kayıt
- `POST /api/auth/login` - Firma giriş
- `POST /api/admin/login` - Admin giriş
- `GET /api/companies` - Firmalar
- `GET /api/companies/nearest` - En yakın firmalar
- `GET /api/address/cities` - İller
- `GET /api/address/districts/{id}` - İlçeler
- `POST /api/tickets` - Ticket oluştur

### Protected Endpoints (Token Gerekir)
- `GET /api/profile` - Profil
- `PUT /api/companies/me` - Profil güncelle
- `GET /api/tickets` - Ticket listesi
- `PUT /api/tickets/{id}/status` - Durum güncelle
- `POST /api/towtrucks` - Çekici ekle
- `GET /api/towtrucks/my` - Çekicilerim
- `PUT /api/admin/address` - Adres güncelle (Admin)
- `GET /api/admin/tickets` - Tüm ticket'lar (Admin)

---

## DOCKER VE DEPLOYMENT

### docker-compose.yml

**Servisler:**
1. **postgres**: PostgreSQL veritabanı
2. **nearest-api**: .NET API

**Özellikler:**
- PostgreSQL 15 Alpine (hafif)
- Volume persistence
- Network isolation
- Port mapping

### Dockerfile

**Multi-stage Build:**
1. Build stage: SDK ile derleme
2. Publish stage: Production build
3. Runtime stage: ASP.NET runtime

**Avantajlar:**
- Küçük imaj boyutu
- Cache optimization
- Güvenlik

---

## VERİ AKIŞI ÖRNEĞİ

### Senaryo: Kullanıcı En Yakın Firmaları Arıyor

```
1. Kullanıcı POST /api/companies/nearest?lat=41.0082&lon=29.0094
   ↓
2. CompaniesController.GetNearestCompanies()
   ↓
3. LocationService.GetNearestCompaniesAsync()
   ↓
4. Veritabanından aktif firmaları çek
   ↓
5. Her firma için CalculateDistance() ile mesafe hesapla
   ↓
6. Mesafeye göre sırala, limit kadar al
   ↓
7. CompanyDto listesi döndür
   ↓
8. JSON response
```

### Senaryo: Firma Kayıt

```
1. POST /api/auth/register
   ↓
2. AuthController.Register()
   ↓
3. Email/telefon unique kontrolü
   ↓
4. AutoMapper: CompanyRegistrationDto → Company
   ↓
5. SHA256 şifre hash'leme
   ↓
6. Veritabanına kaydet
   ↓
7. JwtService.GenerateToken(company)
   ↓
8. AuthResponseDto döndür (token + firma bilgisi)
```

---

## ÖNEMLI NOTLAR

### Performance
- Haversine hesaplaması her istemde çalışır
- Caching eklenebilir
- Database indexing kritik

### Güvenlik
- Production'da JWT key değiştirilmeli
- HTTPS zorunlu
- Email şifreleri environment variable'dan

### Genişletilebilirlik
- Repository pattern: Farklı DB desteği
- Service abstraction: Mock testing
- AutoMapper: Kolay DTO değişiklikleri

---

## SONUÇ

Nearest API, modern .NET mimarisi ile tasarlanmış, ölçeklenebilir ve güvenli bir oto kurtarma firmaları platformudur. Konum tabanlı arama, JWT authentication, role-based authorization ve Docker desteği ile production-ready bir uygulamadır.

**Geliştirici Notları:**
- Tüm controller'lar XML dokümantasyon içerir
- Swagger UI ile test edilebilir
- Migration'lar otomatik uygulanır
- Default admin otomatik oluşturulur
- Email bildirimleri aktif

**İletişim:**
- Email: nearestmek@gmail.com
- Platform: .NET 8 Web API
- Database: PostgreSQL 15

---

*Bu dokümantasyon Nearest API v1.0 için hazırlanmıştır.*


