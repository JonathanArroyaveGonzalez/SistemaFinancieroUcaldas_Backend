# 🔐 Sistema de Autenticación SAPFIAI

## 📋 Tabla de Contenidos
- [Arquitectura General](#arquitectura-general)
- [Endpoints de Autenticación](#endpoints-de-autenticación)
- [Flujo de Autenticación](#flujo-de-autenticación)
- [Entidades de Base de Datos](#entidades-de-base-de-datos)
- [Servicios Implementados](#servicios-implementados)
- [Variables de Entorno](#variables-de-entorno)
- [Configuración de Email (Brevo)](#configuración-de-email-brevo)
- [Ejemplos de Uso](#ejemplos-de-uso)

---

## 🏗️ Arquitectura General

El sistema sigue la arquitectura **Clean Architecture** con las siguientes capas:

```
┌─────────────────────────────────────────────────────────────┐
│                        Web (API)                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Endpoints/Controllers                               │    │
│  │  - Authentication.cs                                 │    │
│  │  - Users.cs                                          │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Application                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Commands & Queries (CQRS con MediatR)              │    │
│  │  - LoginCommand                                      │    │
│  │  - RegisterCommand                                   │    │
│  │  - ValidateTwoFactorCommand                         │    │
│  │  - RefreshTokenCommand                              │    │
│  │  - ForgotPasswordCommand                            │    │
│  │  - ResetPasswordCommand                             │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Interfaces                                          │    │
│  │  - IIdentityService                                  │    │
│  │  - IJwtTokenGenerator                               │    │
│  │  - ITwoFactorService                                │    │
│  │  - IEmailService                                    │    │
│  │  - IAuditLogService                                 │    │
│  │  - IRefreshTokenService                             │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Infrastructure                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Services                                            │    │
│  │  - IdentityService                                   │    │
│  │  - JwtTokenGenerator                                │    │
│  │  - TwoFactorService                                 │    │
│  │  - BrevoEmailService                                │    │
│  │  - AuditLogService                                  │    │
│  │  - RefreshTokenService                              │    │
│  │  - AuthenticationOperations                         │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Data (Entity Framework Core)                       │    │
│  │  - ApplicationDbContext                             │    │
│  │  - Migrations                                        │    │
│  │  - Configurations                                    │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Identity (ASP.NET Core Identity)                   │    │
│  │  - ApplicationUser                                   │    │
│  │  - UserManager                                       │    │
│  │  - RoleManager                                       │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Domain                                │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Entities                                            │    │
│  │  - AuditLog                                          │    │
│  │  - RefreshToken                                      │    │
│  │  - Permission                                        │    │
│  │  - RolePermission                                    │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

---

## 🌐 Endpoints de Autenticación

### Base URL: `/api/Authentication`

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `POST` | `/register` | Registro de nuevo usuario | ❌ Público |
| `POST` | `/login` | Inicio de sesión (genera código 2FA) | ❌ Público |
| `POST` | `/verify-2fa` | Verificar código 2FA | ❌ Público |
| `POST` | `/refresh-token` | Renovar token de acceso | ❌ Público |
| `POST` | `/forgot-password` | Solicitar reset de contraseña | ❌ Público |
| `POST` | `/reset-password` | Restablecer contraseña | ❌ Público |
| `POST` | `/revoke-token` | Revocar refresh token | ✅ Requerida |
| `POST` | `/enable-2fa` | Habilitar 2FA | ✅ Requerida |
| `GET` | `/audit-logs` | Ver logs de auditoría | ✅ Admin |
| `GET` | `/audit-logs/user/{userId}` | Ver logs de usuario | ✅ Requerida |

---

## 🔄 Flujo de Autenticación

### 1️⃣ Registro de Usuario

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API
    participant IS as IdentityService
    participant ES as EmailService
    participant DB as Database

    C->>API: POST /register {email, password, userName}
    API->>IS: CreateUserAsync()
    IS->>DB: Crear usuario en AspNetUsers
    IS->>IS: Asignar rol "User"
    API->>ES: SendRegistrationConfirmationAsync()
    ES-->>C: Email de bienvenida
    API->>DB: Guardar AuditLog (REGISTER_SUCCESS)
    API-->>C: {success: true, userId, message}
```

### 2️⃣ Login con 2FA

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API
    participant AO as AuthOperations
    participant 2FA as TwoFactorService
    participant ES as EmailService
    participant JWT as JwtGenerator
    participant DB as Database

    C->>API: POST /login {email, password}
    API->>AO: VerifyCredentialsAsync()
    AO->>DB: Verificar contraseña
    AO-->>API: {isValid, userId, email}
    
    API->>JWT: GenerateToken(requires2FA: true)
    JWT-->>API: Token temporal
    
    API->>2FA: GenerateAndSendTwoFactorCodeAsync()
    2FA->>DB: Guardar código en AspNetUsers
    2FA->>ES: SendTwoFactorCodeAsync()
    ES-->>C: Email con código 2FA
    
    API->>DB: Guardar AuditLog (LOGIN_PENDING_2FA)
    API-->>C: {token: "temporal", message: "Código enviado"}
```

### 3️⃣ Verificación 2FA

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API
    participant JWT as JwtGenerator
    participant 2FA as TwoFactorService
    participant RT as RefreshTokenService
    participant DB as Database

    C->>API: POST /verify-2fa {code, token}
    API->>JWT: ValidateToken(token)
    JWT-->>API: {userId, email}
    
    API->>2FA: ValidateTwoFactorCodeAsync(userId, code)
    2FA->>DB: Verificar código y expiración
    2FA-->>API: true
    
    API->>2FA: ClearTwoFactorCodeAsync()
    2FA->>DB: Limpiar código del usuario
    
    API->>JWT: GenerateToken(requires2FA: false)
    JWT-->>API: Token final
    
    API->>RT: GenerateRefreshTokenAsync()
    RT->>DB: Guardar RefreshToken
    RT-->>API: refreshToken
    
    API->>DB: Guardar AuditLog (LOGIN_SUCCESS)
    API-->>C: {token: "final", refreshToken, user}
```

### 4️⃣ Refresh Token

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API
    participant RT as RefreshTokenService
    participant JWT as JwtGenerator
    participant DB as Database

    C->>API: POST /refresh-token {refreshToken}
    API->>RT: ValidateRefreshTokenAsync()
    RT->>DB: Buscar token válido
    RT-->>API: {userId, isValid}
    
    API->>RT: RevokeAndRotateAsync()
    RT->>DB: Revocar token actual
    RT->>DB: Crear nuevo RefreshToken
    
    API->>JWT: GenerateToken()
    JWT-->>API: Nuevo access token
    
    API-->>C: {token, refreshToken}
```

---

## 🗃️ Entidades de Base de Datos

### Diagrama ER

```
┌─────────────────────┐       ┌─────────────────────┐
│    AspNetUsers      │       │    AspNetRoles      │
├─────────────────────┤       ├─────────────────────┤
│ Id (PK)             │       │ Id (PK)             │
│ UserName            │       │ Name                │
│ Email               │◄──────│ NormalizedName      │
│ PasswordHash        │       │ ConcurrencyStamp    │
│ IsTwoFactorEnabled  │       └─────────────────────┘
│ TwoFactorCode       │                │
│ TwoFactorCodeExp    │                │
│ LastLoginDate       │       ┌────────┴────────┐
│ LastLoginIp         │       │ AspNetUserRoles │
└─────────────────────┘       ├─────────────────┤
         │                    │ UserId (FK)     │
         │                    │ RoleId (FK)     │
         │                    └─────────────────┘
         │
         ▼
┌─────────────────────┐       ┌─────────────────────┐
│    AuditLogs        │       │   RefreshTokens     │
├─────────────────────┤       ├─────────────────────┤
│ Id (PK)             │       │ Id (PK)             │
│ UserId              │       │ UserId              │
│ Action              │       │ Token               │
│ IpAddress           │       │ ExpiresAt           │
│ UserAgent           │       │ CreatedAt           │
│ Timestamp           │       │ RevokedAt           │
│ Details             │       │ IsRevoked           │
│ Status              │       │ ReplacedByToken     │
│ ErrorMessage        │       │ CreatedByIp         │
│ ResourceId          │       │ RevokedByIp         │
│ ResourceType        │       └─────────────────────┘
└─────────────────────┘

┌─────────────────────┐       ┌─────────────────────┐
│    Permissions      │       │  RolePermissions    │
├─────────────────────┤       ├─────────────────────┤
│ Id (PK)             │◄──────│ Id (PK)             │
│ Name                │       │ RoleId              │
│ Description         │       │ PermissionId (FK)   │
│ Module              │       │ AssignedAt          │
│ IsActive            │       │ AssignedBy          │
│ CreatedAt           │       └─────────────────────┘
└─────────────────────┘
```

### ApplicationUser (Extendido de IdentityUser)

```csharp
public class ApplicationUser : IdentityUser
{
    // Campos adicionales para 2FA
    public bool IsTwoFactorEnabled { get; set; }
    public string? TwoFactorCode { get; set; }
    public DateTime? TwoFactorCodeExpiration { get; set; }
    
    // Información de último login
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIp { get; set; }
    
    // Relaciones
    public virtual ICollection<AuditLog> AuditLogs { get; set; }
}
```

---

## 🛠️ Servicios Implementados

### 1. JwtTokenGenerator
Genera y valida tokens JWT.

```csharp
public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, IList<string> roles, bool requiresTwoFactorVerification = false);
    string GenerateRefreshToken();
    (bool IsValid, string? UserId, string? Email) ValidateToken(string token);
}
```

### 2. TwoFactorService
Maneja la generación y validación de códigos 2FA.

```csharp
public interface ITwoFactorService
{
    Task<bool> GenerateAndSendTwoFactorCodeAsync(string userId);
    Task<bool> ValidateTwoFactorCodeAsync(string userId, string code);
    Task ClearTwoFactorCodeAsync(string userId);
    Task<bool> IsTwoFactorEnabledAsync(string userId);
}
```

### 3. RefreshTokenService
Gestiona los refresh tokens.

```csharp
public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(string userId, string? ipAddress = null);
    Task<(bool IsValid, string? UserId)> ValidateRefreshTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress = null);
    Task<string?> RotateRefreshTokenAsync(string oldToken, string? ipAddress = null);
    Task RevokeAllUserTokensAsync(string userId, string? ipAddress = null);
}
```

### 4. AuditLogService
Registra todas las acciones del sistema.

```csharp
public interface IAuditLogService
{
    Task LogActionAsync(string userId, string action, string? ipAddress = null, 
        string? userAgent = null, string? details = null, string status = "SUCCESS", 
        string? errorMessage = null, string? resourceId = null, string? resourceType = null);
    Task<List<AuditLogDto>> GetUserAuditLogsAsync(string userId, int skip = 0, int take = 50);
    Task<List<AuditLogDto>> GetAuditLogsAsync(string? action = null, int skip = 0, int take = 50);
}
```

### 5. BrevoEmailService
Envía emails usando la API de Brevo (Sendinblue).

```csharp
public interface IEmailService
{
    Task<bool> SendTwoFactorCodeAsync(string email, string code, string userName);
    Task<bool> SendLoginConfirmationAsync(string email, string userName, string ipAddress, DateTime loginDate);
    Task<bool> SendSecurityAlertAsync(string email, string userName, string action, string ipAddress);
    Task<bool> SendRegistrationConfirmationAsync(string email, string userName);
    Task<bool> SendPasswordResetAsync(string email, string userName, string resetToken);
}
```

---

## 🔧 Variables de Entorno

Crear archivo `.env` en la raíz del proyecto:

```env
# =============================================================================
# DATABASE
# =============================================================================
CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=tu-servidor;Database=tu-db;User Id=usuario;Password=password;

# =============================================================================
# BREVO (SENDINBLUE) EMAIL SERVICE
# =============================================================================
API_KEY_BREVO=tu-api-key-de-brevo
BREVO_SENDER_EMAIL=tu-email-verificado@dominio.com
BREVO_SENDER_NAME=NombreApp

# =============================================================================
# TWO-FACTOR AUTHENTICATION
# =============================================================================
TWO_FACTOR_EXPIRATION_MINUTES=10

# =============================================================================
# JWT CONFIGURATION
# =============================================================================
JWT__KEY=TuClaveSecretaDeAlMenos32CaracteresParaJWT
JWT__ISSUER=NombreApp
JWT__AUDIENCE=NombreApp-Users
JWT__EXPIREMINUTES=60
JWT__REFRESHTOKENEXPIRATIONDAYS=7

# =============================================================================
# APPLICATION
# =============================================================================
APP__BASEURL=https://tu-dominio.com
ASPNETCORE_ENVIRONMENT=Development
```

---

## 📧 Configuración de Email (Brevo)

### Pasos para configurar Brevo:

1. **Crear cuenta** en [https://app.brevo.com](https://app.brevo.com)

2. **Obtener API Key:**
   - Ir a Configuración → Claves API
   - Crear nueva clave
   - Copiar la clave generada

3. **Verificar email remitente:**
   - Ir a Configuración → Remitentes
   - Añadir el email que usarás como remitente
   - Verificar haciendo clic en el enlace del email recibido

4. **Configurar variables de entorno:**
   ```env
   API_KEY_BREVO=xkeysib-xxxxx
   BREVO_SENDER_EMAIL=tu-email-verificado@gmail.com
   BREVO_SENDER_NAME=SAPFIAI
   ```

---

## 📝 Ejemplos de Uso

### 1. Registro

```bash
curl -X POST 'https://localhost:5001/api/Authentication/register' \
  -H 'Content-Type: application/json' \
  -d '{
    "email": "usuario@ejemplo.com",
    "password": "MiPassword123!",
    "userName": "usuario"
  }'
```

**Respuesta:**
```json
{
  "success": true,
  "userId": "b4489164-7071-4446-9765-e1ac185a361d",
  "message": "Usuario registrado exitosamente"
}
```

### 2. Login

```bash
curl -X POST 'https://localhost:5001/api/Authentication/login' \
  -H 'Content-Type: application/json' \
  -d '{
    "email": "usuario@ejemplo.com",
    "password": "MiPassword123!"
  }'
```

**Respuesta:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "b4489164-7071-4446-9765-e1ac185a361d",
    "email": "usuario@ejemplo.com",
    "userName": "usuario"
  },
  "message": "Código de verificación enviado a tu correo electrónico"
}
```

### 3. Verificar 2FA

```bash
curl -X POST 'https://localhost:5001/api/Authentication/verify-2fa' \
  -H 'Content-Type: application/json' \
  -d '{
    "code": "123456",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }'
```

**Respuesta:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "user": {
    "id": "b4489164-7071-4446-9765-e1ac185a361d",
    "email": "usuario@ejemplo.com"
  },
  "message": "Verificación exitosa"
}
```

### 4. Usar Token en Peticiones Protegidas

```bash
curl -X GET 'https://localhost:5001/api/Authentication/audit-logs' \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
```

### 5. Refresh Token

```bash
curl -X POST 'https://localhost:5001/api/Authentication/refresh-token' \
  -H 'Content-Type: application/json' \
  -d '{
    "refreshToken": "abc123..."
  }'
```

---

## 🚀 Comandos Útiles

### Ejecutar la aplicación
```bash
dotnet run --project src\Web
```

### Crear migración
```bash
dotnet ef migrations add NombreMigracion --project src\Infrastructure --startup-project src\Web --output-dir Data/Migrations
```

### Aplicar migraciones
```bash
dotnet ef database update --project src\Infrastructure --startup-project src\Web
```

### Build sin NSwag (desarrollo)
```bash
$env:SkipNSwag="True"; dotnet build
```

---

## 📊 Acciones de Auditoría Registradas

| Acción | Descripción | Status |
|--------|-------------|--------|
| `LOGIN_SUCCESS` | Login completado exitosamente | SUCCESS |
| `LOGIN_FAILED` | Credenciales inválidas | FAILED |
| `LOGIN_PENDING_2FA` | Esperando verificación 2FA | PENDING |
| `LOGIN_2FA_SEND_FAILED` | Error al enviar código 2FA | FAILED |
| `REGISTER_SUCCESS` | Registro exitoso | SUCCESS |
| `REGISTER_FAILED` | Error en registro | FAILED |
| `PASSWORD_RESET_REQUESTED` | Solicitud de reset | SUCCESS |
| `PASSWORD_RESET_SUCCESS` | Contraseña cambiada | SUCCESS |
| `PASSWORD_RESET_FAILED` | Error al cambiar contraseña | FAILED |
| `TOKEN_REFRESH` | Token renovado | SUCCESS |
| `TOKEN_REVOKE` | Token revocado | SUCCESS |
| `2FA_ENABLED` | 2FA habilitado | SUCCESS |
| `2FA_DISABLED` | 2FA deshabilitado | SUCCESS |

---

## 👤 Usuario de Prueba (Seed)

Al iniciar la aplicación en desarrollo, se crea automáticamente:

| Campo | Valor |
|-------|-------|
| Email | `administrator@localhost` |
| Password | `Administrator1!` |
| Rol | `Administrator` |

---

## 📁 Estructura de Archivos

```
src/
├── Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── IAuditLogService.cs
│   │   │   ├── IAuthenticationOperations.cs
│   │   │   ├── IEmailService.cs
│   │   │   ├── IIdentityService.cs
│   │   │   ├── IJwtTokenGenerator.cs
│   │   │   ├── IRefreshTokenService.cs
│   │   │   └── ITwoFactorService.cs
│   │   └── Models/
│   │       └── AuditLogDto.cs
│   └── Users/
│       ├── Commands/
│       │   ├── EnableTwoFactor/
│       │   ├── ForgotPassword/
│       │   ├── Login/
│       │   ├── RefreshToken/
│       │   ├── Register/
│       │   ├── ResetPassword/
│       │   ├── RevokeToken/
│       │   └── ValidateTwoFactor/
│       └── Queries/
│           └── GetAuditLogsQuery.cs
├── Domain/
│   ├── Entities/
│   │   ├── AuditLog.cs
│   │   ├── Permission.cs
│   │   ├── RefreshToken.cs
│   │   └── RolePermission.cs
│   └── Enums/
│       └── AuthEnums.cs
├── Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── ApplicationDbContextInitialiser.cs
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── Identity/
│   │   ├── ApplicationUser.cs
│   │   └── IdentityService.cs
│   └── Services/
│       ├── AuditLogService.cs
│       ├── AuthenticationOperations.cs
│       ├── BrevoEmailService.cs
│       ├── JwtTokenGenerator.cs
│       ├── RefreshTokenService.cs
│       └── TwoFactorService.cs
└── Web/
    ├── Endpoints/
    │   └── Authentication.cs
    ├── Services/
    │   └── CurrentUser.cs
    ├── Program.cs
    └── appsettings.json
```

---

## ✅ Checklist de Seguridad

- [x] Contraseñas hasheadas con ASP.NET Core Identity
- [x] JWT con expiración configurable
- [x] Refresh tokens con rotación automática
- [x] 2FA por email obligatorio en login
- [x] Código 2FA con expiración (10 minutos por defecto)
- [x] Auditoría de todas las acciones de autenticación
- [x] Registro de IP y User-Agent en logs
- [x] Variables sensibles en archivo .env (no en código)
- [x] Validación de entrada con FluentValidation
- [x] Políticas de contraseña fuertes

---

## 📞 Soporte

Para reportar problemas o sugerencias, crear un issue en el repositorio:
[https://github.com/JonathanArroyaveGonzalez/SistemaFinancieroUcaldas_Backend](https://github.com/JonathanArroyaveGonzalez/SistemaFinancieroUcaldas_Backend)
