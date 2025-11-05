# Hospital Management System - Clean Architecture

Bu proje, Clean Architecture prensiplerine uygun olarak geliştirilmiş bir hastane yönetim sistemi API'sidir.

## 🏗️ Proje Yapısı

```
HospitalApp/
├── HospitalApp.Domain/          # Domain katmanı (Entities)
│   └── Entities/
│       ├── Patient.cs
│       └── User.cs
├── HospitalApp.Application/     # Application katmanı (Services, DTOs)
│   ├── DTOs/
│   │   └── AuthDto.cs
│   └── Services/
│       ├── AuthService.cs
│       └── PatientService.cs
├── HospitalApp.Infrastructure/  # Infrastructure katmanı (Data Access)
│   └── Data/
│       └── ApplicationDbContext.cs
└── HospitalApp.WebAPI/         # Presentation katmanı (Controllers, Services)
    ├── Controllers/
    │   ├── AuthController.cs
    │   └── PatientsController.cs
    ├── Services/
    │   └── JwtService.cs
    ├── Migrations/
    │   ├── InitialCreate.cs
    │   ├── RemoveRoleFromUser.cs
    │   └── ApplicationDbContextModelSnapshot.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    └── HospitalApp.WebAPI.csproj
```

## 🚀 Teknolojiler

- **.NET 9.0**
- **Entity Framework Core 9.0**
- **SQL Server**
- **JWT Authentication**
- **Swagger/OpenAPI**
- **BCrypt** (Password Hashing)
- **MediatR** (CQRS Pattern)

## 📋 Özellikler

### 🔐 Authentication & Authorization
- JWT Token tabanlı kimlik doğrulama
- Kullanıcı kayıt ve giriş sistemi
- Username veya email ile giriş desteği

### 👥 Kullanıcı Yönetimi
- Kullanıcı kayıt/giriş
- Şifre hashleme (BCrypt)
- Gelişmiş hata mesajları

### 🏥 Hasta Yönetimi
- Hasta ekleme/düzenleme/silme
- Hasta listesi görüntüleme
- Hasta detay görüntüleme

## 🛠️ Kurulum

### Gereksinimler
- .NET 9.0 SDK
- SQL Server
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Repository'yi klonlayın:**
```bash
git clone https://github.com/yaprakasln/HospitalApp.git
cd HospitalApp
```

2. **Veritabanı bağlantısını yapılandırın:**
`HospitalApp.WebAPI/appsettings.json` dosyasında connection string'i güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CleanHospitalDB;User ID=SA;Password=YourPassword;TrustServerCertificate=True"
  }
}
```

3. **Migration'ları çalıştırın:**
```bash
cd HospitalApp.WebAPI
dotnet ef database update
```

4. **Projeyi çalıştırın:**
```bash
dotnet run
```

5. **Swagger UI'ya erişin:**
```
http://localhost:5055/swagger
```

## 🔑 API Kullanımı

### Authentication

#### Kayıt Ol
```bash
POST /api/auth/register
{
  "username": "yaprak",
  "email": "yaprak@hospital.com",
  "password": "123456"
}
```

#### Giriş Yap (Username veya Email ile)
```bash
POST /api/auth/login
{
  "username": "yaprak",  # veya email adresi
  "password": "123456"
}
```

### Hasta İşlemleri

#### Hasta Listesi
```bash
GET /api/patients
Authorization: Bearer {token}
```

#### Hasta Ekle
```bash
POST /api/patients
Authorization: Bearer {token}
{
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "age": 35
}
```

## 🏛️ Clean Architecture Katmanları

### 🎯 Domain Katmanı
- **Sorumluluk:** İş kuralları ve entity'ler
- **Bağımlılık:** Hiçbir katmana bağımlı değil
- **İçerik:** Patient, User entity'leri

### 🔧 Application Katmanı
- **Sorumluluk:** İş mantığı ve servisler
- **Bağımlılık:** Sadece Domain katmanına bağımlı
- **İçerik:** AuthService, PatientService, DTOs

### 🗄️ Infrastructure Katmanı
- **Sorumluluk:** Veri erişimi ve dış servisler
- **Bağımlılık:** Domain ve Application katmanlarına bağımlı
- **İçerik:** ApplicationDbContext, Repository'ler

### � WlebAPI Katmanı
- **Sorumluluk:** HTTP istekleri ve yanıtları
- **Bağımlılık:** Application ve Infrastructure katmanlarına bağımlı
- **İçerik:** Controller'lar, Program.cs, JWT Service

## 🔒 Güvenlik

- **JWT Token:** 24 saat geçerlilik süresi
- **Password Hashing:** BCrypt ile güvenli şifreleme
- **Flexible Login:** Username veya email ile giriş
- **HTTPS:** Güvenli iletişim

## � Son Günscellemeler

### v1.1.0 - Auth Sistemi İyileştirmeleri
- ✅ Login sistemi username veya email ile çalışacak şekilde güncellendi
- ✅ Register işleminde email alanı eklendi
- ✅ Hata mesajları daha açıklayıcı hale getirildi
- ✅ JWT token generation düzeltildi
- ✅ Gereksiz dosyalar temizlendi
- ✅ Database migration'ları güncellendi

## �‍💻 Gelikştirici

**Proje Sahibi:** Yaprak Aslan
- GitHub: [@yaprakasln](https://github.com/yaprakasln)

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
