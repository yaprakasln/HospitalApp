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


---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
